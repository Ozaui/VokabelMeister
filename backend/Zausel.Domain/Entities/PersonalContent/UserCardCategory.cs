namespace Zausel.Domain.Entities.PersonalContent;

// BaseEntity'den BİLİNÇLİ olarak türemez — WordCategory ile AYNI desen: UserCard↔sistem Category
// arasında saf M:N bağ satırı, kendi audit/soft-delete geçmişi yok.
public class UserCardCategory
{
    public int Id { get; set; }
    public int UserCardId { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
}
