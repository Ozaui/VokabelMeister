namespace Zausel.Application.DTOs.Words;

public record WordCreateRequest(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    List<WordTranslationRequest> Translations);
