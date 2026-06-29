// ─────────────────────────────────────────────────────────────────────────────
// BaseEntity.cs
//
// AMAÇ: Tüm domain entity'lerinin türeceği ortak taban sınıf.
// NEDEN: Id, zaman damgaları ve soft delete alanlarını tek yerden yönetir;
//        entity'lere tekrar tekrar aynı alanları yazmayı önler.
// BAĞIMLILIKLAR: Yok — saf C# sınıfı, hiçbir dış pakete bağımlı değil.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities;

public abstract class BaseEntity
{
    // AMAÇ: Her kaydın birincil anahtarı.
    // NEDEN: EF Core bu adı otomatik PK olarak tanır; ayrıca konfigürasyon gerekmez.
    public int Id { get; set; }

    // AMAÇ: Kaydın ilk oluşturulduğu an (UTC).
    // NEDEN: Audit trail ve sıralama için zorunlu; UTC kullanmak zaman dilimi hatalarını önler.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // AMAÇ: Kaydın son güncellendiği an (UTC).
    // NEDEN: Repository<T>.UpdateAsync her çağrıda bu alanı otomatik günceller;
    //        manuel set etmeye gerek kalmaz.
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // AMAÇ: Kaydın silinip silinmediğini gösteren bayrak (soft delete).
    // NEDEN: Fiziksel silme yerine bu bayrağı set ederiz; böylece silinmiş kayıtlar
    //        DB'de kalır, geri yüklenebilir ve audit geçmişi korunur.
    //        WordLearnerDbContext'teki global query filter bu bayrağa göre filtreler.
    public bool IsDeleted { get; set; }

    // AMAÇ: Kaydın soft delete yapıldığı an (UTC). Silinmemişse null.
    // NEDEN: Kullanıcı hesabı 30 günlük grace period sonrası anonimleştirme (A-10)
    //        gibi süre bazlı işlemlerde bu tarih kullanılır.
    public DateTime? DeletedAt { get; set; }
}
