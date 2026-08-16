namespace WordLearner.Domain.Entities.Content;

// BaseEntity'den BİLİNÇLİ olarak türemez — statik seed/referans tablosu (de, tr), audit/soft-delete
// anlamsız; DATABASE_SCHEMA/Icerik.md'deki Languages tablosuyla birebir eşleşir.
public class Language
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
