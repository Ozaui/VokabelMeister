namespace Zausel.Application.DTOs.Words;

public record WordUpdateRequest(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    List<WordTranslationRequest> Translations,
    List<int>? CategoryIds);
