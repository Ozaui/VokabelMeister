namespace Zausel.Domain.Entities.PersonalContent;

// BaseEntity'den BİLİNÇLİ olarak türemez — WordCategory/UserCardCategory ile AYNI desen: UserCard↔
// kişisel UserCategory arasında saf M:N bağ satırı. UserCategoryId FK'si bilinçli NO ACTION —
// Users silindiğinde hem UserCards hem UserCategories üzerinden CASCADE ulaşılsaydı "multiple
// cascade paths" hatası olurdu; UserCardId cascade zincirini taşır (bkz. Kisisel_Icerik.md).
public class UserCardUserCategory
{
    public int Id { get; set; }
    public int UserCardId { get; set; }
    public int UserCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
}
