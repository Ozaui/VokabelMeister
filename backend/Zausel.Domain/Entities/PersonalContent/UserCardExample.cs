namespace Zausel.Domain.Entities.PersonalContent;

// BaseEntity'den BİLİNÇLİ olarak türemez — UserCard'ın sahibi dışında kimse görmediği/düzenlemediği
// bir alt satır, kimin değiştirdiği/geri alınabilirliği anlamsız; yaşam döngüsü tamamen UserCard'a
// bağlı (cascade silinir). UpdatedAt'in varlığı WordExample'dan (BaseEntity) FARKLI: kullanıcı kendi
// örnek cümlesini yerinde düzenleyebilir (mutable), ama bunun için tam audit zinciri gerekmez.
public class UserCardExample
{
    public int Id { get; set; }
    public int UserCardId { get; set; }
    public string SentenceFront { get; set; } = string.Empty;
    public string SentenceBack { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
