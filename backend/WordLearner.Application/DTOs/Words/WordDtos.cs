using System.Text.Json;
using WordLearner.Application.DTOs.Categories;

namespace WordLearner.Application.DTOs.Words;

public record WordDetailDto(
    string? Pronunciation,
    string? AudioUrl,
    string? Notes,
    string? CommonMistakes,
    JsonElement? GrammarData
);

public record WordExampleDto(int Id, string SentenceText, string Level, string ExampleType, int? PairedExampleId);

public record WordTranslationSummaryDto(string LanguageCode, string Text);

public record WordTranslationDto(
    string LanguageCode,
    string Text,
    string? Definition,
    WordDetailDto? WordDetail,
    IReadOnlyList<WordExampleDto> Examples
);

public record WordConceptListItemDto(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationSummaryDto> Translations,
    IReadOnlyList<WordCategorySummaryDto> Categories
);

public record WordConceptDetailDto(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationDto> Translations,
    IReadOnlyList<WordCategorySummaryDto> Categories
);

public record UnmatchedWordConceptDto(
    int WordConceptId,
    string LanguageCode,
    string Text,
    string? Definition,
    string PartOfSpeech,
    string DifficultyLevel,
    int? SuggestedMatchConceptId
);
