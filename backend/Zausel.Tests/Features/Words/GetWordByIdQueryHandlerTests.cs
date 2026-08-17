using FluentAssertions;
using Moq;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Words;

public class GetWordByIdQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();

    private GetWordByIdQueryHandler CreateHandler() => new(_wordConceptRepository.Object);

    [Fact]
    public async Task Handle_ConceptFound_ReturnsWordResponse()
    {
        // ARRANGE
        var concept = new WordConcept { Id = 1, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var word = new Word { Id = 1, WordConceptId = 1, LanguageId = 1, Text = "Baum", Definition = "ağaç" };
        var aggregate = new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])]);
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(aggregate);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetWordByIdQuery(1), default);

        // ASSERT
        result.WordConceptId.Should().Be(1);
        result.Translations.Should().ContainSingle(t => t.Text == "Baum");
    }

    [Fact]
    public async Task Handle_ConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((WordConceptAggregate?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetWordByIdQuery(99), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
