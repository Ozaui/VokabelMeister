// ─────────────────────────────────────────────────────────────────────────────
// Category.cs
//
// AMAÇ: Dilden bağımsız kategori çekirdeği — sınırsız derinlikte kendine referanslı
//       (self-ref) hiyerarşi (ör. "Yiyecek" → "Meyve"). Her dildeki adı ayrı bir
//       CategoryTranslation satırıdır (bkz. DATABASE_SCHEMA/Icerik.md).
// NEDEN: Kategori adı gibi dile özel bir alan (NameDE/NameTR) doğrudan buraya
//        EKLENMEZ — WordConcept/Word ayrımıyla birebir aynı çoklu dil deseni
//        (CLAUDE.md "Çoklu dil" kuralı).
// BAĞIMLILIKLAR: BaseEntity, CategoryTranslation (1:N), WordCategory (1:N), self-ref (ParentCategoryId).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Categories;

public class Category : BaseEntity
{
    // AMAÇ: Üst kategori — NULL ise kök seviye kategori.
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    // AMAÇ: Aynı seviyedeki kategoriler arasında gösterim sırası.
    public int DisplayOrder { get; set; }

    // AMAÇ: Admin panel/mobil UI'da gösterilen simge adı (ör. "food").
    public string? Icon { get; set; }

    // AMAÇ: UI'da kategoriyi temsil eden renk (ör. "#FF6B6B").
    public string? Color { get; set; }

    // AMAÇ: Bu kategorinin önerildiği zorluk aralığı — geçerli değerler: A1, A2, B1, B2, C1, C2.
    public string? MinLevel { get; set; }
    public string? MaxLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();
}
