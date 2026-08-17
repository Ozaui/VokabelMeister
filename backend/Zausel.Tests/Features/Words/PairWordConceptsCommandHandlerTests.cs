using FluentAssertions;
using Moq;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Words;

public class PairWordConceptsCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private PairWordConceptsCommandHandler CreateHandler() => new(_wordConceptRepository.Object, _activityLogger.Object);

    private static WordConceptAggregate BuildAggregate(int conceptId, int languageId, string languageCode, string text, int wordId)
    {
        var concept = new WordConcept { Id = conceptId, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = "A1" };
        var language = new Language { Id = languageId, Code = languageCode, Name = languageCode, NativeName = languageCode };
        var word = new Word { Id = wordId, WordConceptId = conceptId, LanguageId = languageId, Text = text, Definition = "anlam" };
        return new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])]);
    }

    [Fact]
    public async Task Handle_PrimaryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((WordConceptAggregate?)null);
        var command = new PairWordConceptsCommand(1, 2, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _wordConceptRepository.Verify(r => r.MoveWordToConceptAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, 1, "de", "Haus", 10));
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((WordConceptAggregate?)null);
        var command = new PairWordConceptsCommand(1, 2, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidPair_MovesOtherWordAndSoftDeletesOtherConcept()
    {
        // ARRANGE
        var primary = BuildAggregate(1, 1, "de", "Haus", 10);
        var other = BuildAggregate(2, 2, "tr", "ev", 20);
        var merged = new WordConceptAggregate(primary.Concept, [.. primary.Translations, .. other.Translations]);
        _wordConceptRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(primary)
            .ReturnsAsync(merged);
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        var command = new PairWordConceptsCommand(1, 2, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().HaveCount(2);
        _wordConceptRepository.Verify(r => r.MoveWordToConceptAsync(20, 1, 1, It.IsAny<CancellationToken>()), Times.Once);
        _wordConceptRepository.Verify(r => r.SoftDeleteConceptOnlyAsync(2, 1, It.IsAny<CancellationToken>()), Times.Once);
        _wordConceptRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidPair_LogsPairWordConceptsActivityWithBothConceptsInOldValue()
    {
        // ARRANGE
        var primary = BuildAggregate(1, 1, "de", "Haus", 10);
        var other = BuildAggregate(2, 2, "tr", "ev", 20);
        var merged = new WordConceptAggregate(primary.Concept, [.. primary.Translations, .. other.Translations]);
        _wordConceptRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(primary)
            .ReturnsAsync(merged);
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(other);
        var command = new PairWordConceptsCommand(1, 2, UserId: 5, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — EntityId HAYATTA kalan (primary) tarafın id'si, OldValue İKİ kavramı da taşıyor
        _activityLogger.Verify(l => l.LogAsync(
            5, "Admin", "PAIR_WORD_CONCEPTS", "Word", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
