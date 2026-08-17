namespace Zausel.Application.DTOs.Words;

public record WordTranslationRequest(
    string LanguageCode,
    string Text,
    string? Definition,
    WordDetailRequest? WordDetail,
    List<WordExampleRequest>? Examples);
