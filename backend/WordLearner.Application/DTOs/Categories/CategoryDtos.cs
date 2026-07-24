namespace WordLearner.Application.DTOs.Categories;

public record CategoryTranslationDto(string LanguageCode, string Name, string? Description);

// Children iç içe (recursive) olduğu için ağaç yapısını doğrudan taşır.
public record CategoryDto(
    int Id,
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    IReadOnlyList<CategoryTranslationDto> Translations,
    int? WordCount,
    IReadOnlyList<CategoryDto> Children
);

// Tam CategoryDto DEĞİL — Children/WordCount bir kelime listesinde gereksiz, yalnızca
// "hangi kategoride, nasıl gösterilir" sorusuna cevap verecek kadarı taşınır.
public record WordCategorySummaryDto(int CategoryId, IReadOnlyList<CategoryTranslationDto> Translations);
