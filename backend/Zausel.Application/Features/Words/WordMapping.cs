using System.Text.Json;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;

namespace Zausel.Application.Features.Words;

// AutoMapper Profile YAZILMADI — WordConceptAggregate dört farklı entity türünü (Concept/Word/
// Language/WordDetail/WordExample) TEK bir iç içe DTO'ya birleştiriyor, AutoMapper'ın düz
// property-property eşlemesi bu iç içe/çoklu-kaynaklı dönüşümü elle yazılmış koddan daha
// OKUNAKLI hale getirmez; dört Handler (Create/Update/GetById/GetWords) bu TEK metodu paylaşır.
public static class WordMapping
{
    public static WordResponse ToResponse(WordConceptAggregate aggregate)
    {
        var concept = aggregate.Concept;
        return new WordResponse(
            concept.Id,
            concept.PartOfSpeech.ToString(),
            concept.DifficultyLevel,
            concept.ImageUrl,
            aggregate.Translations.Select(ToTranslationResponse).ToList());
    }

    private static WordTranslationResponse ToTranslationResponse(WordTranslationAggregate translation) =>
        new(
            translation.Language.Code,
            translation.Word.Text,
            translation.Word.Definition,
            translation.Detail is null ? null : ToDetailResponse(translation.Detail),
            translation.Examples.Select(ToExampleResponse).ToList());

    private static WordDetailResponse ToDetailResponse(WordDetail detail) =>
        new(detail.Pronunciation, detail.Notes, detail.CommonMistakes, ParseGrammarData(detail.GrammarData));

    private static WordExampleResponse ToExampleResponse(WordExample example) =>
        new(example.Id, example.SentenceText, example.Level, example.ExampleType.ToString(), example.PairedExampleId, example.DisplayOrder);

    // JsonDocument IDisposable — RootElement Clone() ile bağımsız bir kopya alınır, aksi halde
    // JsonDocument metodun sonunda dispose edilince Response'a konan JsonElement geçersiz kalırdı.
    private static JsonElement? ParseGrammarData(string? json) =>
        json is null ? null : JsonDocument.Parse(json).RootElement.Clone();
}
