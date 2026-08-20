namespace Zausel.Application.DTOs.Categories;

public record CategoryCreateRequest(
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    List<CategoryTranslationRequest> Translations);
