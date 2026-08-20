namespace Zausel.Application.DTOs.Words;

public record WordResponse(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    List<WordTranslationResponse> Translations,
    List<WordCategoryResponse> Categories);
