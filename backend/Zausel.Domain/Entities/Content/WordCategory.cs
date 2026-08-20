namespace Zausel.Domain.Entities.Content;

// BaseEntity'den BİLİNÇLİ olarak türemez — WordConcept↔Category M:N ara tablosu, UserCardCategories
// (Kisisel_Icerik.md) ile AYNI desen: saf bağ satırı, kendi audit/soft-delete geçmişi yok, ilişki
// silinince satır da (hard) silinir.
public class WordCategory
{
    public int Id { get; set; }
    public int WordConceptId { get; set; }
    public int CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
