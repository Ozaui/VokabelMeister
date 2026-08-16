using MediatR;
using WordLearner.Application.DTOs;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories.Content;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Application.Features.Words;

// categoryId filtresi BİLİNÇLİ olarak YOK — Category/WordCategory entity'leri henüz yazılmadı (A-06),
// o task'ta bu Query'ye geriye dönük eklenecek (API_ENDPOINTS.md §6 notu).
public record GetWordsQuery(string? DifficultyLevel, string? PartOfSpeech, string? Search, int Page, int PageSize)
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
            request.DifficultyLevel, partOfSpeech, request.Search, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<WordResponse>
        {
            Items = page.Items.Select(WordMapping.ToResponse).ToList(),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }
}
