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
using WordLearner.Application.DTOs.Words;
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
            concept.Words.OrderBy(w => w.LanguageId).Select(BuildTranslation).ToList()
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
                .ToList()
        );

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
