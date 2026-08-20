using FluentAssertions;
using Moq;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Words;

public class DeleteWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private DeleteWordCommandHandler CreateHandler() => new(_wordConceptRepository.Object, _activityLogger.Object);

    private static WordConceptAggregate BuildAggregate(int conceptId, string text)
    {
        var concept = new WordConcept { Id = conceptId, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var word = new Word { Id = 1, WordConceptId = conceptId, LanguageId = 1, Text = text, Definition = "ağaç" };
        return new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])], []);
    }

    [Fact]
    public async Task Handle_ConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((WordConceptAggregate?)null);
        var command = new DeleteWordCommand(99, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _wordConceptRepository.Verify(r => r.SoftDeleteConceptCascadeAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidDelete_CallsSoftDeleteCascadeAndReturnsUnit()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(1, "Baum"));
        var command = new DeleteWordCommand(1, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Should().Be(MediatR.Unit.Value);
        _wordConceptRepository.Verify(r => r.SoftDeleteConceptCascadeAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        _wordConceptRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Success_LogsDeleteWordActivityWithOldValueOnly()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(1, "Baum"));
        var command = new DeleteWordCommand(1, UserId: 4, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue silinen kaydın son hâli, NewValue HER ZAMAN null
        _activityLogger.Verify(l => l.LogAsync(
            4, "Admin", "DELETE_WORD", "Word", 1, It.IsAny<object>(), null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
