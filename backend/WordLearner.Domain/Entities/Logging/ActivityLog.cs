// ─────────────────────────────────────────────────────────────────────────────
// ActivityLog.cs
//
// AMAÇ: "Kim ne yaptı" audit kaydı (kelime silindi, rol değişti, kart oluşturuldu vb.).
// NEDEN: BaseEntity'den TÜRETİLMEZ — log tabloları insert-only ve değişmez (CLAUDE.md
//        "Veri katmanı"); soft delete/UpdatedAt/audit-kim-güncelledi alanlarının hiçbiri
//        bir log kaydı için anlamlı değil, bir kayıt yazıldıktan sonra asla değişmez/silinmez.
//        BaseEntity.CreatedByUserId/UpdatedByUserId yalnızca "kaydın şu anki hâlini en son
//        kim etkiledi" sorusunu cevaplar; ActivityLog ise her işlemin TAM geçmişini
//        (OldValue/NewValue JSON diff) ayrı ayrı satırlar hâlinde saklar — ikisi birbirinin
//        yerine geçmez, tamamlar (bkz. wiki/Database/Loglama_Domain.md).
// BAĞIMLILIKLAR: User (N:1, opsiyonel — anonim eylemlerde NULL).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Domain.Entities.Logging;

public class ActivityLog
{
    // AMAÇ: Birincil anahtar. BIGINT — log tabloları BaseEntity.Id (int) kullanmaz,
    //       yüksek hacimli insert-only tablolarda satır sayısı int sınırını aşabilir.
    public long Id { get; set; }

    // AMAÇ: Eylemi yapan kullanıcı. Anonim eylemlerde (ör. başarısız login denemesi
    //       öncesi henüz kimliği bilinmiyorsa) NULL kalır.
    public int? UserId { get; set; }

    // AMAÇ: Kaydın yazıldığı andaki rol (User|Admin). Kullanıcının rolü sonradan
    //       değişse bile bu alan o anki gerçeği donduğu gibi tutar.
    public string? ActorRole { get; set; }

    // AMAÇ: Yapılan eylemin sabit kodu (ör. LOGIN, REGISTER, CREATE_WORD, DELETE_USER_CARD, CHANGE_ROLE).
    // NEDEN: Serbest metin NVARCHAR — enum yapılmadı, çünkü her yeni feature (Word/Category/
    //        UserCard/Class CRUD'ları — bkz. CLAUDE.md "Veri katmanı") kendi Action string'ini
    //        ekler; enum olsaydı her yeni feature bu dosyayı değiştirmek zorunda kalırdı.
    public string Action { get; set; } = string.Empty;

    // AMAÇ: Eylemin hangi entity tipini etkilediği (ör. Word, UserCard, User, Category).
    public string? EntityType { get; set; }

    // AMAÇ: Etkilenen entity'nin Id'si.
    public int? EntityId { get; set; }

    // AMAÇ: Değişiklik öncesi/sonrası durumun JSON serileştirmesi (audit diff).
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // AMAÇ: Kaydın yazıldığı an (UTC). BaseEntity.CreatedAt'in aksine varsayılan
    //       değeri burada tutulur çünkü bu entity BaseEntity'den türemiyor.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // AMAÇ: Navigation property — EF Core Include() ile kullanıcı bilgisine erişim.
    // NEDEN: Tek yönlü ilişki (QrLoginSession ile aynı desen) — User tarafında
    //        karşılık gelen bir koleksiyon eklenmedi, hiçbir akış "kullanıcının log
    //        kayıtları" listesini User üzerinden istemiyor (YAGNI); log görüntüleme
    //        (A-07/B-08) zaten ActivityLog tablosundan UserId'ye göre sorgular.
    public User? User { get; set; }
}
