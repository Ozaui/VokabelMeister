using FluentAssertions;
using Moq;
using Zausel.Application.DTOs;
using Zausel.Application.Features.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Words;

public class GetUnmatchedWordConceptsQueryHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private GetUnmatchedWordConceptsQueryHandler CreateHandler() =>
        new(_wordConceptRepository.Object, _languageRepository.Object);

    [Fact]
    public async Task Handle_LanguageNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepository.Setup(r => r.GetActiveOrderedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([German, Turkish]);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetUnmatchedWordConceptsQuery(99, null, 1, 20), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoOppositeLanguage_ReturnsSuggestionsAllNull()
    {
        // ARRANGE — sistemde yalnızca TEK aktif dil var, "karşı dil" YOK
        _languageRepository.Setup(r => r.GetActiveOrderedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([German]);
        var unmatched = new UnmatchedWordAggregate(1, "Anrufbeantworter", "telesekreter", PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedAsync(1, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UnmatchedWordAggregate> { Items = [unmatched], TotalCount = 1, Page = 1, PageSize = 20 });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(1, null, 1, 20), default);

        // ASSERT
        result.Items.Should().ContainSingle().Which.SuggestedMatchConceptId.Should().BeNull();
        _wordConceptRepository.Verify(r => r.GetUnmatchedPoolAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DefinitionTokenMatchesCandidateText_ReturnsSuggestedMatchConceptId()
    {
        // ARRANGE — "de" tarafındaki kelimenin Definition'ı ("telesekreter, ansaföron") "tr" havuzundaki bir Text ile eşleşiyor
        _languageRepository.Setup(r => r.GetActiveOrderedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([German, Turkish]);
        var unmatched = new UnmatchedWordAggregate(1, "Anrufbeantworter", "telesekreter, ansaföron", PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedAsync(1, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UnmatchedWordAggregate> { Items = [unmatched], TotalCount = 1, Page = 1, PageSize = 20 });
        var candidate = new UnmatchedWordAggregate(3, "telesekreter", "Anrufbeantworter", PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedPoolAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync([candidate]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(1, null, 1, 20), default);

        // ASSERT
        result.Items.Should().ContainSingle().Which.SuggestedMatchConceptId.Should().Be(3);
    }

    [Fact]
    public async Task Handle_TextMatchesCandidateDefinitionToken_ReturnsSuggestedMatchConceptIdViaReverseDirection()
    {
        // ARRANGE — "tr" tarafındaki kelimenin Definition'ı BOŞ ama Text'i, "de" havuzundaki bir adayın Definition'ında geçiyor
        _languageRepository.Setup(r => r.GetActiveOrderedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([German, Turkish]);
        var unmatched = new UnmatchedWordAggregate(3, "telesekreter", null, PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedAsync(2, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UnmatchedWordAggregate> { Items = [unmatched], TotalCount = 1, Page = 1, PageSize = 20 });
        var candidate = new UnmatchedWordAggregate(1, "Anrufbeantworter", "telesekreter, ansaföron", PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedPoolAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([candidate]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(2, null, 1, 20), default);

        // ASSERT
        result.Items.Should().ContainSingle().Which.SuggestedMatchConceptId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoMatchingCandidate_ReturnsNullSuggestion()
    {
        // ARRANGE
        _languageRepository.Setup(r => r.GetActiveOrderedAsync(It.IsAny<CancellationToken>())).ReturnsAsync([German, Turkish]);
        var unmatched = new UnmatchedWordAggregate(1, "Baum", "ağaç", PartOfSpeech.Noun, "A1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedAsync(1, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UnmatchedWordAggregate> { Items = [unmatched], TotalCount = 1, Page = 1, PageSize = 20 });
        var candidate = new UnmatchedWordAggregate(3, "telesekreter", "Anrufbeantworter", PartOfSpeech.Noun, "B1");
        _wordConceptRepository.Setup(r => r.GetUnmatchedPoolAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync([candidate]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUnmatchedWordConceptsQuery(1, null, 1, 20), default);

        // ASSERT
        result.Items.Should().ContainSingle().Which.SuggestedMatchConceptId.Should().BeNull();
    }
}
