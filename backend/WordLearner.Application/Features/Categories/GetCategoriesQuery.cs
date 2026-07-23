// ─────────────────────────────────────────────────────────────────────────────
// GetCategoriesQuery.cs
//
// AMAÇ: GET /categories — hiyerarşik (ağaç) kategori listesi.
// NEDEN includeWordCount opsiyonel: `ICategoryRepository.GetWordCountsAsync` tüm
//        WordCategories tablosunu GROUP BY ile tarar — bu ekstra bir sorgu, yalnızca
//        istemci gerçekten `includeWordCount=true` gönderdiğinde (ör. B-04'ün admin
//        listesi "bu kategoride N kelime var" göstermek istediğinde) çalıştırılır.
// BAĞIMLILIKLAR: ICategoryRepository, CategoryDtoBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Categories;

public record GetCategoriesQuery(string? Level, bool IncludeWordCount) : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository) =>
        _categoryRepository = categoryRepository;

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var flat = await _categoryRepository.GetAllWithTranslationsAsync(request.Level, ct);

        var wordCounts = request.IncludeWordCount ? await _categoryRepository.GetWordCountsAsync(ct) : null;

        return CategoryDtoBuilder.BuildTree(flat, wordCounts);
    }
}
