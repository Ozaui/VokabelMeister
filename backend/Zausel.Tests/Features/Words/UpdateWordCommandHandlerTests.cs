using FluentAssertions;
using Moq;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Words;

public class UpdateWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UpdateWordCommandHandler CreateHandler() =>
        new(_wordConceptRepository.Object, _languageRepository.Object, _activityLogger.Object);

    private static WordConceptAggregate BuildAggregate(int conceptId, string text, string difficultyLevel = "A1")
    {
        var concept = new WordConcept { Id = conceptId, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = difficultyLevel };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var word = new Word { Id = 1, WordConceptId = conceptId, LanguageId = 1, Text = text, Definition = "ağaç" };
        return new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])]);
    }

    [Fact]
    public async Task Handle_ConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((WordConceptAggregate?)null);
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new UpdateWordCommand(99, "Noun", "A1", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesConceptAndReturnsResponse()
    {
        // ARRANGE — eski seviye A1, yeni istek A2 gönderiyor
        var before = BuildAggregate(1, "Baum", "A1");
        var after = BuildAggregate(1, "Baum", "A2");
        _wordConceptRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(before)
            .ReturnsAsync(after);
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _wordConceptRepository.Setup(r => r.FindWordAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 1, WordConceptId = 1, LanguageId = 1, Text = "Baum" });
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Word?)null);
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new UpdateWordCommand(1, "Noun", "A2", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.DifficultyLevel.Should().Be("A2");
        _wordConceptRepository.Verify(r => r.UpdateWordAsync(It.IsAny<Word>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateTextWithoutForce_ThrowsWordDuplicateException()
    {
        // ARRANGE — güncellenen metin BAŞKA bir Word'de zaten var
        var before = BuildAggregate(1, "Baum");
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(before);
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _wordConceptRepository.Setup(r => r.FindWordAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 1, WordConceptId = 1, LanguageId = 1, Text = "Baum" });
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Strauch", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 9, Text = "Strauch" });
        var translations = new List<WordTranslationRequest> { new("de", "Strauch", "ağaç", null, null) };
        var command = new UpdateWordCommand(1, "Noun", "A1", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<WordDuplicateException>();
    }

    [Fact]
    public async Task Handle_Success_LogsUpdateWordActivityWithOldAndNewValue()
    {
        // ARRANGE
        var before = BuildAggregate(1, "Baum", "A1");
        var after = BuildAggregate(1, "Baum", "A2");
        _wordConceptRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(before)
            .ReturnsAsync(after);
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _wordConceptRepository.Setup(r => r.FindWordAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 1, WordConceptId = 1, LanguageId = 1, Text = "Baum" });
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Word?)null);
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new UpdateWordCommand(1, "Noun", "A2", null, translations, Force: false, UserId: 3, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — hem OldValue hem NewValue dolu (Create'in AKSİNE)
        _activityLogger.Verify(l => l.LogAsync(
            3, "Admin", "UPDATE_WORD", "Word", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
