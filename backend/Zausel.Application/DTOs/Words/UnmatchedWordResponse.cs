namespace Zausel.Application.DTOs.Words;

public record UnmatchedWordResponse(
    int WordConceptId,
    string LanguageCode,
    string Text,
    string PartOfSpeech,
    string DifficultyLevel,
    int? SuggestedMatchConceptId);
