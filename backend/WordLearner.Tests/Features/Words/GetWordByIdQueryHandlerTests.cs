// ─────────────────────────────────────────────────────────────────────────────
// GetWordByIdQueryHandlerTests.cs
//
// AMAÇ: GetWordByIdQueryHandler'ın bulunan kavramı tam detay DTO'suna
//       çevirdiğini ve bulunamayan Id'de 404 fırlattığını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class GetWordByIdQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();

    private GetWordByIdQueryHandler CreateHandler() => new(_wordConceptRepo.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    /// <summary>
    /// GetById_ExistingConcept_ReturnsDetailDtoWithTranslations
    ///
    /// AMAÇ: Var olan bir kavramın dillerinin (WordDetail/örnekler dahil)
    ///       tam detay DTO'suna doğru şekilde projekte edildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetById_ExistingConcept_ReturnsDetailDtoWithTranslations()
    {
        // ARRANGE
        var concept = new WordConcept
        {
            Id = 10,
            PartOfSpeech = "Noun",
            DifficultyLevel = "A1",
            Words = new List<Word>
            {
                new()
                {
                    LanguageId = German.Id,
                    Language = German,
                    Text = "Tisch",
                    WordExamples = new List<WordExample>(),
                },
            },
        };
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concept);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetWordByIdQuery(10), default);

        // ASSERT
        result.WordConceptId.Should().Be(10);
        result.Translations.Should().ContainSingle(t => t.LanguageCode == "de" && t.Text == "Tisch");
    }

    /// <summary>
    /// GetById_NotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Var olmayan bir WordConcept Id'si verilirse EntityNotFoundException
    ///       (404) fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetById_NotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetWordByIdQuery(999), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
