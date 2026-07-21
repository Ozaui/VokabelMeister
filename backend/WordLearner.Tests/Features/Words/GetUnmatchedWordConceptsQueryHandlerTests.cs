// ─────────────────────────────────────────────────────────────────────────────
// GetUnmatchedWordConceptsQueryHandlerTests.cs
//
// AMAÇ: GetUnmatchedWordConceptsQueryHandler'ın filtre+sayfa parametrelerini
//       repository'ye ilettiğini, çoklu Definition token'larına bölünerek
//       öneri arandığını (İKİ yönde: Definition↔Text) ve öneri bulunamayınca
//       null döndüğünü doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class GetUnmatchedWordConceptsQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();

    private GetUnmatchedWordConceptsQueryHandler CreateHandler() => new(_wordConceptRepo.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private static WordConcept UnmatchedConcept(int id, Language language, string text, string? definition) =>
        new()
        {
            Id = id,
            PartOfSpeech = "Conjunction",
            DifficultyLevel = "A1",
            Words = new List<Word>
            {
                new()
                {
                    LanguageId = language.Id,
                    Language = language,
                    Text = text,
                    Definition = definition,
                },
            },
        };

    /// <summary>
    /// GetUnmatched_ForwardsFiltersToRepository
    ///
    /// AMAÇ: Query'deki LanguageId/Search/Page/PageSize'ın repository.GetUnmatchedPagedAsync'e
    ///       AYNEN iletildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetUnmatched_ForwardsFiltersToRepository()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedPagedAsync(German.Id, "Tisch", 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept>(), 0, 2, 10));
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedOtherLanguagePoolAsync(German.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WordConcept>());
        var handler = CreateHandler();
        var query = new GetUnmatchedWordConceptsQuery(German.Id, "Tisch", 2, 10);

        // ACT
        var result = await handler.Handle(query, default);

        // ASSERT
        result.Page.Should().Be(2);
        _wordConceptRepo.Verify(
            r => r.GetUnmatchedPagedAsync(German.Id, "Tisch", 2, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// GetUnmatched_DefinitionMatchesPoolText_SetsSuggestedMatchConceptId
    ///
    /// AMAÇ: Adayın Definition'ı ("ama, fakat, ancak") virgülle token'lara bölünüp
    ///       her biri karşı havuzun Text'ine karşı denendiğinde, ikinci token'ın
    ///       (tek string olarak asla eşleşmeyecek "fakat") eşleştiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetUnmatched_DefinitionMatchesPoolText_SetsSuggestedMatchConceptId()
    {
        // ARRANGE
        var candidate = UnmatchedConcept(10, German, "aber", "ama, fakat, ancak");
        var poolMatch = UnmatchedConcept(20, Turkish, "fakat", null);
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedPagedAsync(German.Id, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept> { candidate }, 1, 1, 20));
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedOtherLanguagePoolAsync(German.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WordConcept> { poolMatch });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(German.Id, null), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.WordConceptId == 10 && i.SuggestedMatchConceptId == 20);
    }

    /// <summary>
    /// GetUnmatched_PoolDefinitionMatchesCandidateText_SetsSuggestedMatchConceptId
    ///
    /// AMAÇ: Ters yönü doğrulamak — adayın Text'i, havuzdaki bir kavramın
    ///       Definition token'larından biriyle eşleştiğinde de öneri bulunur.
    /// </summary>
    [Fact]
    public async Task GetUnmatched_PoolDefinitionMatchesCandidateText_SetsSuggestedMatchConceptId()
    {
        // ARRANGE
        var candidate = UnmatchedConcept(10, German, "aber", null);
        var poolMatch = UnmatchedConcept(20, Turkish, "fakat", "aber, doch");
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedPagedAsync(German.Id, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept> { candidate }, 1, 1, 20));
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedOtherLanguagePoolAsync(German.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WordConcept> { poolMatch });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(German.Id, null), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.WordConceptId == 10 && i.SuggestedMatchConceptId == 20);
    }

    /// <summary>
    /// GetUnmatched_NoOverlap_SuggestedMatchConceptIdIsNull
    ///
    /// AMAÇ: Havuzda hiçbir Text/Definition örtüşmesi yoksa öneri null döner
    ///       (liste manuel taranır, hata fırlatılmaz).
    /// </summary>
    [Fact]
    public async Task GetUnmatched_NoOverlap_SuggestedMatchConceptIdIsNull()
    {
        // ARRANGE
        var candidate = UnmatchedConcept(10, German, "Anrufbeantworter", null);
        var poolItem = UnmatchedConcept(20, Turkish, "masa", null);
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedPagedAsync(German.Id, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept> { candidate }, 1, 1, 20));
        _wordConceptRepo
            .Setup(r => r.GetUnmatchedOtherLanguagePoolAsync(German.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WordConcept> { poolItem });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(German.Id, null), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.WordConceptId == 10 && i.SuggestedMatchConceptId == null);
    }
}
