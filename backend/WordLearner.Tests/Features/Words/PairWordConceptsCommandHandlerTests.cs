using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class PairWordConceptsCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private PairWordConceptsCommandHandler CreateHandler() =>
        new(_wordConceptRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private static WordConcept Concept(int id, string partOfSpeech, string difficultyLevel, Language language, string text) =>
        new()
        {
            Id = id,
            PartOfSpeech = partOfSpeech,
            DifficultyLevel = difficultyLevel,
            Words = new List<Word> { new() { WordConceptId = id, LanguageId = language.Id, Language = language, Text = text } },
        };

    [Fact]
    public async Task Pair_HappyPath_ReturnsMergedConceptFromRepository()
    {
        // ARRANGE
        var primary = Concept(12, "Noun", "A1", German, "Mann");
        var other = Concept(87, "Noun", "A1", Turkish, "Erkek");
        var merged = Concept(12, "Noun", "A1", German, "Mann");
        merged.Words.Add(new Word { WordConceptId = 12, LanguageId = Turkish.Id, Language = Turkish, Text = "Erkek" });

        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(primary);
        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(87, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        _wordConceptRepo
            .Setup(r => r.PairAsync(12, 87, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(merged);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new PairWordConceptsCommand(12, 87) { UserId = 7 }, default);

        // ASSERT
        result.WordConceptId.Should().Be(12);
        result.Translations.Should().HaveCount(2);
        _wordConceptRepo.Verify(r => r.PairAsync(12, 87, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Pair_ConflictingPartOfSpeech_MergesWithoutThrowing()
    {
        // ARRANGE
        var primary = Concept(12, "Verb", "A1", German, "anrufen");
        var other = Concept(87, "Noun", "A1", Turkish, "telefon görüşmesi");
        var merged = Concept(12, "Verb", "A1", German, "anrufen");
        merged.Words.Add(new Word { WordConceptId = 12, LanguageId = Turkish.Id, Language = Turkish, Text = "telefon görüşmesi" });

        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(primary);
        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(87, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        _wordConceptRepo
            .Setup(r => r.PairAsync(12, 87, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(merged);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new PairWordConceptsCommand(12, 87), default);
        var result = await act();

        // ASSERT
        await act.Should().NotThrowAsync();
        result.PartOfSpeech.Should().Be("Verb");
    }

    [Fact]
    public async Task Pair_PrimaryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new PairWordConceptsCommand(999, 87), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _wordConceptRepo.Verify(
            r => r.PairAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Pair_OtherConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        var primary = Concept(12, "Noun", "A1", German, "Mann");
        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(primary);
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new PairWordConceptsCommand(12, 999), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Pair_Success_LogsPairWordConceptsActivity()
    {
        // ARRANGE
        var primary = Concept(12, "Noun", "A1", German, "Mann");
        var other = Concept(87, "Noun", "A1", Turkish, "Erkek");
        var merged = Concept(12, "Noun", "A1", German, "Mann");
        merged.Words.Add(new Word { WordConceptId = 12, LanguageId = Turkish.Id, Language = Turkish, Text = "Erkek" });

        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(primary);
        _wordConceptRepo.Setup(r => r.GetWithTranslationsAsync(87, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        _wordConceptRepo
            .Setup(r => r.PairAsync(12, 87, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(merged);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new PairWordConceptsCommand(12, 87) { UserId = 7, ActorRole = "Admin" }, default);

        // ASSERT
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    7,
                    "Admin",
                    "PAIR_WORD_CONCEPTS",
                    "WordConcept",
                    12,
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    null,
                    null,
                    default
                ),
            Times.Once
        );
    }
}
