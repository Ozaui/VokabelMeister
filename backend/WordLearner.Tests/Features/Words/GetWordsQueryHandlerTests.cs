// ─────────────────────────────────────────────────────────────────────────────
// GetWordsQueryHandlerTests.cs
//
// AMAÇ: GetWordsQueryHandler'ın filtre+sayfa parametrelerini repository'ye
//       olduğu gibi ilettiğini ve sonucu liste DTO'suna (WordDetail/örnek
//       TAŞIMADAN) çevirdiğini doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class GetWordsQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();

    private GetWordsQueryHandler CreateHandler() => new(_wordConceptRepo.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    /// <summary>
    /// GetWords_ForwardsFiltersAndReturnsMappedPage
    ///
    /// AMAÇ: Query'deki DifficultyLevel/PartOfSpeech/Search/Page/PageSize'ın
    ///       repository.GetPagedAsync'e AYNEN iletildiğini ve dönen sayfanın
    ///       liste DTO'larına (yalnızca dil+metin) çevrildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetWords_ForwardsFiltersAndReturnsMappedPage()
    {
        // ARRANGE
        var concept = new WordConcept
        {
            Id = 10,
            PartOfSpeech = "Noun",
            DifficultyLevel = "A1",
            Words = new List<Word> { new() { LanguageId = German.Id, Language = German, Text = "Tisch" } },
        };
        _wordConceptRepo
            .Setup(r => r.GetPagedAsync("A1", "Noun", "Tisch", 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept> { concept }, 1, 2, 10));
        var handler = CreateHandler();
        var query = new GetWordsQuery("A1", "Noun", "Tisch", 2, 10);

        // ACT
        var result = await handler.Handle(query, default);

        // ASSERT
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(2);
        result.Items.Should().ContainSingle(i => i.WordConceptId == 10 && i.Translations[0].Text == "Tisch");
    }
}
