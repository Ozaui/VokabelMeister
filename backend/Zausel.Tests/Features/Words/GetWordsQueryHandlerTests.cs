using FluentAssertions;
using Moq;
using Zausel.Application.DTOs;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;

namespace Zausel.Tests.Features.Words;

public class GetWordsQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();

    private GetWordsQueryHandler CreateHandler() => new(_wordConceptRepository.Object);

    private static WordConceptAggregate BuildAggregate(int conceptId, string text)
    {
        var concept = new WordConcept { Id = conceptId, PartOfSpeech = PartOfSpeech.Noun, DifficultyLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var word = new Word { Id = conceptId, WordConceptId = conceptId, LanguageId = 1, Text = text, Definition = "anlam" };
        return new WordConceptAggregate(concept, [new WordTranslationAggregate(word, language, null, [])], []);
    }

    [Fact]
    public async Task Handle_ValidPartOfSpeechFilter_PassesParsedEnumToRepository()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetPagedAsync("A1", PartOfSpeech.Noun, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConceptAggregate> { Items = [], TotalCount = 0, Page = 1, PageSize = 20 });
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetWordsQuery("A1", "Noun", null, null, 1, 20), default);

        // ASSERT — "Noun" string'i PartOfSpeech.Noun enum'ına DOĞRU parse edilip repository'ye geçirildi
        _wordConceptRepository.Verify(r => r.GetPagedAsync("A1", PartOfSpeech.Noun, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPartOfSpeechFilter_PassesNullToRepository()
    {
        // ARRANGE — geçersiz bir tür adı ("Gecersiz") — Enum.TryParse başarısız olur, filtre UYGULANMAZ
        _wordConceptRepository.Setup(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConceptAggregate> { Items = [], TotalCount = 0, Page = 1, PageSize = 20 });
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetWordsQuery(null, "Gecersiz", null, null, 1, 20), default);

        // ASSERT
        _wordConceptRepository.Verify(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryIdFilterProvided_PassesToRepository()
    {
        // ARRANGE — A-06: categoryId filtresi repository'ye AYNEN geçiyor
        _wordConceptRepository.Setup(r => r.GetPagedAsync(null, null, null, 3, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConceptAggregate> { Items = [], TotalCount = 0, Page = 1, PageSize = 20 });
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetWordsQuery(null, null, null, 3, 1, 20), default);

        // ASSERT
        _wordConceptRepository.Verify(r => r.GetPagedAsync(null, null, null, 3, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsAggregates_ReturnsPagedResultMappedToWordResponses()
    {
        // ARRANGE
        var page = new PagedResult<WordConceptAggregate>
        {
            Items = [BuildAggregate(1, "Baum"), BuildAggregate(2, "Haus")],
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };
        _wordConceptRepository.Setup(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>())).ReturnsAsync(page);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetWordsQuery(null, null, null, null, 1, 20), default);

        // ASSERT
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Translations.Single().Text).Should().BeEquivalentTo("Baum", "Haus");
    }
}
