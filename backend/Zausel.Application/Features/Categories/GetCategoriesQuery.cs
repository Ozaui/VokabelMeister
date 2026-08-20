using MediatR;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Interfaces.Repositories.Content;

namespace Zausel.Application.Features.Categories;

public record GetCategoriesQuery(string? Level, bool IncludeWordCount) : IRequest<List<CategoryResponse>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    private static readonly string[] Levels = ["A1", "A2", "B1", "B2", "C1", "C2"];

    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var all = await _categoryRepository.GetAllAsync(cancellationToken);
        var filtered = request.Level is null
            ? all
            : all.Where(a => IsInRange(request.Level, a.Category.MinLevel, a.Category.MaxLevel)).ToList();

        var wordCounts = request.IncludeWordCount
            ? await _categoryRepository.GetActiveWordCountsAsync(cancellationToken)
            : null;

        // Filtrelenmiş üst kategori kümenin DIŞINDA kaldıysa (ör. level filtresiyle elenmişse), o çocuk
        // ağacın KÖKÜ olarak gösterilir — sessizce KAYBOLMAZ. ILookup null anahtarı DESTEKLER
        // (Dictionary'nin AKSİNE), kök kategoriler (ParentCategoryId=null) bu yüzden ToLookup ile.
        var filteredIds = filtered.Select(a => a.Category.Id).ToHashSet();
        var childrenByParent = filtered
            .ToLookup(a => a.Category.ParentCategoryId is int parentId && filteredIds.Contains(parentId) ? parentId : (int?)null);

        return BuildTree(null, childrenByParent, wordCounts);
    }

    private static List<CategoryResponse> BuildTree(
        int? parentId, ILookup<int?, CategoryAggregate> childrenByParent, Dictionary<int, int>? wordCounts)
    {
        return childrenByParent[parentId]
            .OrderBy(c => c.Category.DisplayOrder)
            .Select(c => CategoryMapping.ToResponse(c, wordCounts?.GetValueOrDefault(c.Category.Id, 0)) with
            {
                Children = BuildTree(c.Category.Id, childrenByParent, wordCounts)
            })
            .ToList();
    }

    // MinLevel/MaxLevel bir ARALIK (ör. yalnızca MinLevel="A2" set edilmişse üst sınır YOK, C2'ye
    // kadar her seviyeden istenen kelime bu kategoride görünebilir) — tek bir eşitlik kontrolü DEĞİL.
    private static bool IsInRange(string level, string? minLevel, string? maxLevel)
    {
        var levelIndex = Array.IndexOf(Levels, level);
        if (levelIndex < 0)
            return true;

        var minIndex = minLevel is null ? 0 : Array.IndexOf(Levels, minLevel);
        var maxIndex = maxLevel is null ? Levels.Length - 1 : Array.IndexOf(Levels, maxLevel);
        return levelIndex >= minIndex && levelIndex <= maxIndex;
    }
}
