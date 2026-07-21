// ─────────────────────────────────────────────────────────────────────────────
// WordDtos.cs
//
// AMAÇ: `GET /words`, `GET /words/{id}`, `POST/PUT /words` yanıtlarının DTO'ları.
// NEDEN: Liste (WordConceptListItemDto) ve detay (WordConceptDetailDto) AYRI
//        DTO'lar — liste yalnızca Text seviyesinde (WordTranslationSummaryDto),
//        detay ise WordDetail+WordExample'larıyla TAM translation (WordTranslationDto)
//        taşır; liste ekranının WordDetail/WordExample'a ihtiyacı yok, gereksiz
//        veri taşımaktan kaçınılır. `categories[]`/`userProgress` alanları
//        (API_ENDPOINTS.md §5 örneğinde var) A-06 (Kategoriler) ve C-03 (SRS) henüz
//        yazılmadığı için BİLİNÇLİ olarak bu DTO'larda YOK — o task'lar geldiğinde eklenecek.
// BAĞIMLILIKLAR: Yok (saf DTO'lar).
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;

namespace WordLearner.Application.DTOs.Words;

// AMAÇ: Bir Word'ün dile özel gramer/telaffuz bilgisi.
public record WordDetailDto(
    string? Pronunciation,
    string? AudioUrl,
    string? Notes,
    string? CommonMistakes,
    JsonElement? GrammarData
);

// AMAÇ: Bir Word'e ait tek bir örnek cümle.
public record WordExampleDto(int Id, string SentenceText, string Level, string ExampleType, int? PairedExampleId);

// AMAÇ: Liste ekranında bir dilin YALNIZCA metni (WordDetail/örnekler YOK).
public record WordTranslationSummaryDto(string LanguageCode, string Text);

// AMAÇ: Detay ekranında bir dilin TAM içeriği.
public record WordTranslationDto(
    string LanguageCode,
    string Text,
    string? Definition,
    WordDetailDto? WordDetail,
    IReadOnlyList<WordExampleDto> Examples
);

// AMAÇ: `GET /words` liste satırı.
public record WordConceptListItemDto(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationSummaryDto> Translations
);

// AMAÇ: `GET /words/{id}`, `POST /words`, `PUT /words/{id}` yanıtı.
public record WordConceptDetailDto(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationDto> Translations
);

// AMAÇ: `GET /words/unmatched` liste satırı — bir dilde eşleşmemiş (tek Word'lü)
//       kavram + karşı dilin eşleşmemiş havuzunda önerilen eşleşme adayı
//       (bkz. Icerik.md "Eşleştirme", WordMatchSuggestionResolver).
public record UnmatchedWordConceptDto(
    int WordConceptId,
    string LanguageCode,
    string Text,
    string? Definition,
    string PartOfSpeech,
    string DifficultyLevel,
    int? SuggestedMatchConceptId
);
