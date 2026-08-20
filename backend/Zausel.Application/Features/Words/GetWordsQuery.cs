using MediatR;
using Zausel.Application.DTOs;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Enums.Content;

namespace Zausel.Application.Features.Words;

public record GetWordsQuery(string? DifficultyLevel, string? PartOfSpeech, string? Search, int? CategoryId, int Page, int PageSize)
    : IRequest<PagedResult<WordResponse>>;

public class GetWordsQueryHandler : IRequestHandler<GetWordsQuery, PagedResult<WordResponse>>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetWordsQueryHandler(IWordConceptRepository wordConceptRepository) => _wordConceptRepository = wordConceptRepository;

    public async Task<PagedResult<WordResponse>> Handle(GetWordsQuery request, CancellationToken cancellationToken)
    {
        var partOfSpeech = request.PartOfSpeech is not null && Enum.TryParse<PartOfSpeech>(request.PartOfSpeech, out var parsed)
            ? parsed
            : (PartOfSpeech?)null;

        var page = await _wordConceptRepository.GetPagedAsync(
            request.DifficultyLevel, partOfSpeech, request.Search, request.CategoryId, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<WordResponse>
        {
            Items = page.Items.Select(WordMapping.ToResponse).ToList(),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }
}
