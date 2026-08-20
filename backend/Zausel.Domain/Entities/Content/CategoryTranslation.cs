namespace Zausel.Domain.Entities.Content;

// BaseEntity'den BİLİNÇLİ olarak türemez — Category'nin CASCADE ile silinen dil-başına-1-satır
// çevirisi, Language gibi kendi audit/soft-delete geçmişi olmayan bir alt-satır (DATABASE_SCHEMA/
// Icerik.md'deki CategoryTranslations SQL'i hiç CreatedAt/UpdatedAt taşımıyor).
public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
