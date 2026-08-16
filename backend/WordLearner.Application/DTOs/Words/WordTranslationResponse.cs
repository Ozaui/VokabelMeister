namespace WordLearner.Application.DTOs.Words;

public record WordTranslationResponse(
    string LanguageCode,
    string Text,
    string? Definition,
    WordDetailResponse? WordDetail,
    List<WordExampleResponse> Examples);
