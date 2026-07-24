using WordLearner.Domain.Entities.Words;

namespace WordLearner.Domain.Entities.Categories;

public class CategoryTranslation : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
