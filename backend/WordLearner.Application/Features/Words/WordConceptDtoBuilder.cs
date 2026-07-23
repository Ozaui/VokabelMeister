// ─────────────────────────────────────────────────────────────────────────────
// WordConceptDtoBuilder.cs
//
// AMAÇ: WordConcept (+ Words/WordDetail/WordExample) entity ağacını DTO'lara
//       (WordConceptDetailDto/WordConceptListItemDto) çeviren paylaşılan yardımcı.
// NEDEN: CLAUDE.md'nin koşullu AutoMapper kuralı — bu bir düz 1:1 Entity→DTO
//        dönüşümü DEĞİL, WordConcept+Word+WordDetail+WordExample+Language'ı
//        `translations[]` şeklinde BİRLEŞTİREN bir projeksiyon; AutoMapper Profile
//        yerine elle inşa edilir (QrLogin DTO'larıyla aynı karar). CreateWordCommand/
//        UpdateWordCommand/GetWordByIdQuery/GetWordsQuery handler'larının HEPSİ bu
//        TEK noktayı kullanır, dönüşüm mantığı tekrarlanmaz.
// BAĞIMLILIKLAR: WordConcept/Word entity'leri, WordDtos.cs.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.DTOs.Words;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public static class WordConceptDtoBuilder
{
    // AMAÇ: Tam detay DTO'su — her dilin WordDetail/örnekleriyle birlikte.
    public static WordConceptDetailDto BuildDetail(WordConcept concept) =>
        new(
            concept.Id,
            concept.PartOfSpeech,
            concept.DifficultyLevel,
            concept.ImageUrl,
            concept.Words.OrderBy(w => w.LanguageId).Select(BuildTranslation).ToList(),
            BuildCategories(concept)
        );

    // AMAÇ: Liste satırı DTO'su — yalnızca dil+metin (WordDetail/örnekler taşınmaz).
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

    // AMAÇ: Bir WordConcept'in bağlı olduğu kategorilerin HAFİF özetini kurar (A-06) —
    //       CategoryDtoBuilder'daki tam `CategoryDto` DEĞİL, WordCategorySummaryDto
    //       (bkz. CategoryDtos.cs "NEDEN" — bir kelime listesinde Children/WordCount
    //       gereksiz).
    // NEDEN wc.Category.Id (navigasyon), wc.CategoryId (skaler FK) DEĞİL: yeni eklenen
    //       bir WordCategory'de (CreateWordCommand/UpdateWordCommand) yalnızca `Category`
    //       navigasyonu set edilir — WordEntityBuilder'ın `Word.Language = language`
    //       deseniyle AYNI karar (LanguageId DEĞİL). Skaler FK, EF Core'un fixup'ı
    //       yalnızca gerçek bir DbContext SaveChangesAsync'i sırasında doldurur; bu
    //       metot her zaman navigasyon üzerinden okuyarak hem SaveChanges ÖNCESİ
    //       (bu metodun kendisi tam olarak bu anda çağrılıyor) hem SONRASI doğru sonuç verir.
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

    // AMAÇ: DB'de string olarak saklanan GrammarData JSON'unu, yanıtta gerçek bir
    //       JSON nesnesi (string olarak KAÇIRILMIŞ/escaped değil) olarak döndürür.
    // NEDEN: `.Clone()` — JsonDocument.Parse'ın ürettiği JsonDocument bu metottan
    //        çıkışta implicit olarak GC'ye bırakılıyor; Clone() olmadan RootElement
    //        yalnızca kendi JsonDocument'i YAŞADIĞI sürece güvenli sayılır, Clone()
    //        ile JsonDocument'ten bağımsız, kalıcı bir kopya alınır.
    private static JsonElement? ParseGrammarData(string? grammarDataJson) =>
        string.IsNullOrWhiteSpace(grammarDataJson) ? null : JsonDocument.Parse(grammarDataJson).RootElement.Clone();
}
