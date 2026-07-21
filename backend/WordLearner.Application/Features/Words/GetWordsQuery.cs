// ─────────────────────────────────────────────────────────────────────────────
// GetWordsQuery.cs
//
// AMAÇ: GET /words — filtre+sayfalı kelime kavramı listesi.
// NEDEN: `categoryId` filtresi (API_ENDPOINTS.md §5) BİLİNÇLİ olarak henüz YOK —
//        A-06 (Kategoriler) yazılmadan Category/WordCategory tabloları mevcut
//        değil; o task tamamlanınca bu Query'ye eklenecek (A-05 planı "Sıra Dışı
//        Notlar"). `search`, kelimenin HERHANGİ bir dildeki Text'inde arar.
// BAĞIMLILIKLAR: IWordConceptRepository, WordConceptDtoBuilder, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

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
    int PageSize = 20
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
