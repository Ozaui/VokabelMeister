using MediatR;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Words;

public record GetWordsQuery(
    string? DifficultyLevel,
    string? PartOfSpeech,
    string? Search,
    int Page = 1,
    int PageSize = 20,
    int? CategoryId = null
) : IRequest<PagedResult<WordConceptListItemDto>>;

public class GetWordsQueryHandler
    : IRequestHandler<GetWordsQuery, PagedResult<WordConceptListItemDto>>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetWordsQueryHandler(IWordConceptRepository wordConceptRepository) =>
        _wordConceptRepository = wordConceptRepository;

    public async Task<PagedResult<WordConceptListItemDto>> Handle(GetWordsQuery request, CancellationToken ct)
    {
        var paged = await _wordConceptRepository.GetPagedAsync(
            request.DifficultyLevel,
            request.PartOfSpeech,
            request.Search,
            request.CategoryId,
            request.Page,
            request.PageSize,
            ct
        );

        return new PagedResult<WordConceptListItemDto>(
            paged.Items.Select(WordConceptDtoBuilder.BuildListItem).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
