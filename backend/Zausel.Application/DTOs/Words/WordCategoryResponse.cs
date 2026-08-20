namespace Zausel.Application.DTOs.Words;

// CategoryResponse'un (DTOs/Categories) HAFİF izdüşümü — kelime kartında hangi kategorilere ait
// olduğunu göstermek için renk/ikon/hiyerarşi gerekmez, yalnızca kimlik + ad.
public record WordCategoryResponse(int CategoryId, List<WordCategoryTranslationResponse> Translations);

public record WordCategoryTranslationResponse(string LanguageCode, string Name);
