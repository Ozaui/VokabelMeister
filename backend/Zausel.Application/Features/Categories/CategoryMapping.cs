using Zausel.Application.DTOs.Categories;
using Zausel.Application.Interfaces.Repositories.Content;

namespace Zausel.Application.Features.Categories;

// AutoMapper Profile YAZILMADI — WordMapping (Features/Words) ile AYNI gerekçe: CategoryAggregate
// Category+çevirileri TEK bir iç içe DTO'ya birleştiriyor, elle yazılmış kod düz property eşlemesinden
// daha okunaklı. wordCount/children[] Handler'da (GetCategoriesQueryHandler) sonradan eklendiği için
// burada YOK — bu metot yalnızca Category+Translations'ı DTO'ya çevirir.
public static class CategoryMapping
{
    public static CategoryResponse ToResponse(CategoryAggregate aggregate, int? wordCount = null) =>
        new(
            aggregate.Category.Id,
            aggregate.Category.ParentCategoryId,
            aggregate.Category.DisplayOrder,
            aggregate.Category.Icon,
            aggregate.Category.Color,
            aggregate.Category.MinLevel,
            aggregate.Category.MaxLevel,
            aggregate.Translations.Select(t => new CategoryTranslationResponse(t.Language.Code, t.Translation.Name, t.Translation.Description)).ToList(),
            wordCount,
            Children: []);
}
