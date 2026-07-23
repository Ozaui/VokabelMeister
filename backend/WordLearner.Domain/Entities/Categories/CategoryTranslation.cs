// ─────────────────────────────────────────────────────────────────────────────
// CategoryTranslation.cs
//
// AMAÇ: Bir Category'nin tek bir dildeki adı (ör. "Essen" de, "Yemek" tr).
// NEDEN: WordConcept→Word ayrımıyla birebir aynı çoklu dil deseni (CLAUDE.md
//        "Çoklu dil" kuralı) — kategori adı Category'ye doğrudan EKLENMEZ.
// BAĞIMLILIKLAR: BaseEntity, Category (N:1), Language (N:1).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Words;

namespace WordLearner.Domain.Entities.Categories;

public class CategoryTranslation : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    // AMAÇ: Kategorinin bu dildeki adı (ör. "Essen").
    public string Name { get; set; } = string.Empty;

    // AMAÇ: Opsiyonel, dile özel kısa açıklama (admin panelde kategori kartında gösterilebilir).
    public string? Description { get; set; }
}
