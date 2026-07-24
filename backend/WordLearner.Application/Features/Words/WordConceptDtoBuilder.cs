using System.Text.Json;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.DTOs.Words;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public static class WordConceptDtoBuilder
{
    public static WordConceptDetailDto BuildDetail(WordConcept concept) =>
        new(
            concept.Id,
            concept.PartOfSpeech,
            concept.DifficultyLevel,
            concept.ImageUrl,
            concept.Words.OrderBy(w => w.LanguageId).Select(BuildTranslation).ToList(),
            BuildCategories(concept)
        );

    public static WordConceptListItemDto BuildListItem(WordConcept concept) =>
        new(
            concept.Id,
            concept.PartOfSpeech,
            concept.DifficultyLevel,
            concept.ImageUrl,
            concept
                .Words.OrderBy(w => w.LanguageId)
                .Select(w => new WordTranslationSummaryDto(w.Language.Code, w.Text))
                .ToList(),
            BuildCategories(concept)
        );

    // wc.Category.Id (navigasyon) — wc.CategoryId DEĞİL: yeni eklenen bir WordCategory'de yalnızca
    // Category navigasyonu set edilir, skaler FK EF Core fixup'ı yalnızca SaveChangesAsync sırasında doldurur.
    private static IReadOnlyList<WordCategorySummaryDto> BuildCategories(WordConcept concept) =>
        concept
            .WordCategories.OrderBy(wc => wc.DisplayOrder)
            .Select(wc => new WordCategorySummaryDto(
                wc.Category.Id,
                wc.Category.Translations.OrderBy(t => t.LanguageId).Select(BuildCategoryTranslation).ToList()
            ))
            .ToList();

    private static CategoryTranslationDto BuildCategoryTranslation(CategoryTranslation t) =>
        new(t.Language.Code, t.Name, t.Description);

    private static WordTranslationDto BuildTranslation(Word word) =>
        new(
            word.Language.Code,
            word.Text,
            word.Definition,
            word.WordDetail is null ? null : BuildWordDetail(word.WordDetail),
            word
                .WordExamples.OrderBy(e => e.DisplayOrder)
                .Select(e => new WordExampleDto(e.Id, e.SentenceText, e.Level, e.ExampleType, e.PairedExampleId))
                .ToList()
        );

    private static WordDetailDto BuildWordDetail(WordDetail detail) =>
        new(
            detail.Pronunciation,
            detail.AudioUrl,
            detail.Notes,
            detail.CommonMistakes,
            ParseGrammarData(detail.GrammarData)
        );

    // .Clone() — Clone() olmadan RootElement yalnızca kendi JsonDocument'i yaşadığı sürece
    // güvenlidir, bu metottan çıkışta JsonDocument GC'ye bırakılır.
    private static JsonElement? ParseGrammarData(string? grammarDataJson) =>
        string.IsNullOrWhiteSpace(grammarDataJson) ? null : JsonDocument.Parse(grammarDataJson).RootElement.Clone();
}
