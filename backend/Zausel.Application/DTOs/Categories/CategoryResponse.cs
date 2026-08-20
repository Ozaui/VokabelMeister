namespace Zausel.Application.DTOs.Categories;

// Children[] — GetCategoriesQueryHandler'ın düz listeden kurduğu ağaç (repository düz döner,
// hiyerarşiyi Handler kurar). WordCount yalnızca ?includeWordCount=true isteğinde dolar, aksi halde null.
public record CategoryResponse(
    int Id,
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    List<CategoryTranslationResponse> Translations,
    int? WordCount,
    List<CategoryResponse> Children);
