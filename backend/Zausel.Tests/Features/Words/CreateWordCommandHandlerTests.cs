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

public class CreateWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateWordCommandHandler CreateHandler() =>
        new(_wordConceptRepository.Object, _languageRepository.Object, _activityLogger.Object);

    private static WordConceptAggregate BuildAggregate(int conceptId, string languageCode, string text)
    {
        var concept = new WordConcept { Id = conceptId, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = "A1" };
        var language = new Language { Id = 1, Code = languageCode, Name = "German", NativeName = "Deutsch" };
        var word = new Word { Id = 1, WordConceptId = conceptId, LanguageId = 1, Text = text, Definition = "ağaç" };
        return new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])]);
    }

    private void SetupGermanLanguage() =>
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });

    [Fact]
    public async Task Handle_SingleLanguageTranslation_CreatesConceptAndReturnsResponse()
    {
        // ARRANGE
        SetupGermanLanguage();
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Word?)null);
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, "de", "Baum"));
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new CreateWordCommand("Noun", "A1", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().ContainSingle(t => t.Text == "Baum");
        _wordConceptRepository.Verify(r => r.AddConceptAsync(It.IsAny<WordConcept>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _wordConceptRepository.Verify(r => r.AddWordAsync(It.Is<Word>(w => w.Text == "Baum"), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateTextWithoutForce_ThrowsWordDuplicateException()
    {
        // ARRANGE — aynı dilde aynı metinli bir Word zaten var, force=false
        SetupGermanLanguage();
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 5, Text = "Baum" });
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new CreateWordCommand("Noun", "A1", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<WordDuplicateException>();
    }

    [Fact]
    public async Task Handle_DuplicateTextWithForce_CreatesAnyway()
    {
        // ARRANGE — force=true duplikat kontrolünü BAYPAS eder
        SetupGermanLanguage();
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 5, Text = "Baum" });
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, "de", "Baum"));
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new CreateWordCommand("Noun", "A1", null, translations, Force: true, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Should().NotBeNull();
        _wordConceptRepository.Verify(r => r.AddWordAsync(It.IsAny<Word>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownLanguageCode_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepository.Setup(r => r.GetByCodeAsync("xx", It.IsAny<CancellationToken>())).ReturnsAsync((Language?)null);
        var translations = new List<WordTranslationRequest> { new("xx", "test", "anlam", null, null) };
        var command = new CreateWordCommand("Noun", "A1", null, translations, Force: false, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_Success_LogsCreateWordActivity()
    {
        // ARRANGE
        SetupGermanLanguage();
        _wordConceptRepository.Setup(r => r.FindWordByLanguageAndTextAsync(1, "Baum", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Word?)null);
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, "de", "Baum"));
        var translations = new List<WordTranslationRequest> { new("de", "Baum", "ağaç", null, null) };
        var command = new CreateWordCommand("Noun", "A1", null, translations, Force: false, UserId: 7, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — CREATE_WORD, OldValue=null (yeni kayıt), NewValue dolu
        _activityLogger.Verify(l => l.LogAsync(
            7, "Admin", "CREATE_WORD", "Word", It.IsAny<int?>(), null, It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
