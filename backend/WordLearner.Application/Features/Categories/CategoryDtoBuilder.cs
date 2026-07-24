using WordLearner.Application.DTOs.Categories;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Features.Categories;

public static class CategoryDtoBuilder
{
    // Level filtresi bir üst kategoriyi eleyip alt kategoriyi geçebilir — bu durumda
    // ParentCategoryId'si flat listede artık yok olan her düğüm kök seviyeye terfi ettirilir.
    public static IReadOnlyList<CategoryDto> BuildTree(
        IReadOnlyList<Category> flat,
        IReadOnlyDictionary<int, int>? wordCounts
    )
    {
        var idSet = flat.Select(c => c.Id).ToHashSet();
        var byParent = flat.ToLookup(c => c.ParentCategoryId);

        var roots = flat.Where(c => c.ParentCategoryId is null || !idSet.Contains(c.ParentCategoryId.Value));

        return roots.OrderBy(c => c.DisplayOrder).Select(c => Build(c, byParent, wordCounts)).ToList();
    }

    public static CategoryDto BuildSingle(Category category) =>
        new(
            category.Id,
            category.ParentCategoryId,
            category.DisplayOrder,
            category.Icon,
            category.Color,
            category.MinLevel,
            category.MaxLevel,
            category.Translations.OrderBy(t => t.LanguageId).Select(BuildTranslation).ToList(),
            WordCount: null,
            Children: Array.Empty<CategoryDto>()
        );

    private static CategoryDto Build(
        Category category,
        ILookup<int?, Category> byParent,
        IReadOnlyDictionary<int, int>? wordCounts
    ) =>
        new(
            category.Id,
            category.ParentCategoryId,
            category.DisplayOrder,
            category.Icon,
            category.Color,
            category.MinLevel,
            category.MaxLevel,
            category.Translations.OrderBy(t => t.LanguageId).Select(BuildTranslation).ToList(),
            wordCounts is null ? null : wordCounts.GetValueOrDefault(category.Id),
            byParent[category.Id]
                .OrderBy(c => c.DisplayOrder)
                .Select(c => Build(c, byParent, wordCounts))
                .ToList()
        );

    private static CategoryTranslationDto BuildTranslation(CategoryTranslation t) =>
        new(t.Language.Code, t.Name, t.Description);
}
