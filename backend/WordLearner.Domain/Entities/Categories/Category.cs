namespace WordLearner.Domain.Entities.Categories;

// Dilden bağımsız kategori çekirdeği, sınırsız derinlikte self-ref hiyerarşi (ParentCategoryId).
// Dile özel ad CategoryTranslation'da tutulur, buraya EKLENMEZ (CLAUDE.md "Çoklu dil" kuralı).
public class Category : BaseEntity
{
    // Null = kök seviye.
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }

    // A1-C2.
    public string? MinLevel { get; set; }
    public string? MaxLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();
}
