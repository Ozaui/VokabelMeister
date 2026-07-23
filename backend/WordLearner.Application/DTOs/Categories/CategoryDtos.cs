// ─────────────────────────────────────────────────────────────────────────────
// CategoryDtos.cs
//
// AMAÇ: `GET /categories`, `POST/PUT /categories` yanıtlarının DTO'ları.
// NEDEN: WordConceptDetailDto/WordConceptListItemDto (A-05) ile AYNI ayrım YOK —
//        Category için tek bir DTO (`CategoryDto`) yeterli, çünkü liste zaten
//        hiyerarşik AĞAÇ döndürüyor (API_ENDPOINTS.md §6: "Hiyerarşik") ve bir
//        kategorinin "detay" görünümü ile "liste satırı" görünümü arasında A-05'teki
//        gibi ağır bir fark (WordDetail/WordExample gibi ekstra alt-varlıklar) YOK.
// BAĞIMLILIKLAR: Yok (saf DTO'lar).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Categories;

// AMAÇ: Bir Category'nin tek bir dildeki adı.
public record CategoryTranslationDto(string LanguageCode, string Name, string? Description);

// AMAÇ: `GET /categories`, `POST/PUT /categories` yanıtındaki bir kategori düğümü —
//       `Children` alanı iç içe (recursive) olduğu için ağaç yapısını doğrudan taşır.
public record CategoryDto(
    int Id,
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    IReadOnlyList<CategoryTranslationDto> Translations,
    int? WordCount,
    IReadOnlyList<CategoryDto> Children
);

// AMAÇ: Bir kelimenin bağlı olduğu kategorinin HAFİF özeti — GET /words yanıtındaki
//       `categories[]` alanında kullanılır (A-06 sonu, WordDtos.cs güncellemesi).
// NEDEN: Tam `CategoryDto` DEĞİL — Children/WordCount gibi alanlar bir kelime
//        listesinde anlamsız (döngüsel/gereksiz veri); yalnızca "bu kelime hangi
//        kategoride, o kategori nasıl gösterilir" sorusuna cevap verecek kadarı taşınır.
public record WordCategorySummaryDto(int CategoryId, IReadOnlyList<CategoryTranslationDto> Translations);
