namespace Zausel.Application.DTOs.Categories;

public record CategoryTranslationRequest(string LanguageCode, string Name, string? Description);
