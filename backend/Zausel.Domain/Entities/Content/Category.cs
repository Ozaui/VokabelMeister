using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.Content;

// Hiyerarşik, dilden bağımsız kategori çekirdeği — ad/açıklama CategoryTranslation'da.
public class Category : BaseEntity
{
    public int? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? MinLevel { get; set; }
    public string? MaxLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
