// ─────────────────────────────────────────────────────────────────────────────
// GetCategoryWordsQuery.cs
//
// AMAÇ: GET /categories/{id}/words — bir kategorinin kelimelerinin sayfalı listesi.
// NEDEN AYRI bir "kategoriye göre kelime" sorgu mantığı YAZILMADI: A-05'in
//        `IWordConceptRepository.GetPagedAsync`'i A-06'da categoryId parametresi
//        alacak şekilde genişletildi (bkz. GetWordsQuery.cs "NEDEN") — bu Query o
//        AYNI metodu, diğer filtreler NULL, yalnızca categoryId dolu şekilde çağırır
//        (YAGNI: tek metot iki tüketici, mantık tekrarlanmaz).
// BAĞIMLILIKLAR: ICategoryRepository (kategori var mı kontrolü), IWordConceptRepository,
//                WordConceptDtoBuilder, PagedResult<T>, EntityNotFoundException.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Features.Categories;

public record GetCategoryWordsQuery(int CategoryId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<WordConceptListItemDto>>;

public class GetCategoryWordsQueryHandler
    : IRequestHandler<GetCategoryWordsQuery, PagedResult<WordConceptListItemDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetCategoryWordsQueryHandler(
        ICategoryRepository categoryRepository,
        IWordConceptRepository wordConceptRepository
    )
    {
        _categoryRepository = categoryRepository;
        _wordConceptRepository = wordConceptRepository;
    }

    public async Task<PagedResult<WordConceptListItemDto>> Handle(GetCategoryWordsQuery request, CancellationToken ct)
    {
        _ =
            await _categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new EntityNotFoundException(typeof(Category), request.CategoryId);

        var paged = await _wordConceptRepository.GetPagedAsync(
            difficultyLevel: null,
            partOfSpeech: null,
            search: null,
            categoryId: request.CategoryId,
            page: request.Page,
            pageSize: request.PageSize,
            ct: ct
        );

        return new PagedResult<WordConceptListItemDto>(
            paged.Items.Select(WordConceptDtoBuilder.BuildListItem).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
