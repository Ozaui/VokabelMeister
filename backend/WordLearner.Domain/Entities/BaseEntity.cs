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

    // AMAÇ: Kaydın son güncellendiği an (UTC). Hiç güncellenmemişse null.
    // NEDEN: Nullable olması "hiç güncellenmedi" ile "CreatedAt anında güncellendi" durumlarını
    //        ayırt eder; WordLearnerDbContext.SaveChangesAsync yalnızca gerçek bir güncelleme
    //        (EntityState.Modified) olduğunda bu alanı set eder, ekleme sırasında dokunmaz.
    public DateTime? UpdatedAt { get; set; }

    // AMAÇ: Kaydın silinip silinmediğini gösteren bayrak (soft delete).
    // NEDEN: Fiziksel silme yerine bu bayrağı set ederiz; böylece silinmiş kayıtlar
    //        DB'de kalır, geri yüklenebilir ve audit geçmişi korunur.
    //        WordLearnerDbContext'teki global query filter bu bayrağa göre filtreler.
    public bool IsDeleted { get; set; }

    // AMAÇ: Kaydın soft delete yapıldığı an (UTC). Silinmemişse null.
    // NEDEN: Kullanıcı hesabı 30 günlük grace period sonrası anonimleştirme (A-10)
    //        gibi süre bazlı işlemlerde bu tarih kullanılır.
    public DateTime? DeletedAt { get; set; }

    // AMAÇ: Kaydı oluşturan kullanıcının Id'si.
    // NEDEN: Repository<T>.AddAsync bu alanı set eder; Auth (A-03) tamamlanana kadar
    //        çağıran taraf userId geçmediği için null kalır — FK, User entity yazılınca (A-03) bağlanır.
    public int? CreatedByUserId { get; set; }

    // AMAÇ: Kaydı en son güncelleyen (soft delete dâhil) kullanıcının Id'si.
    // NEDEN: Repository<T>.UpdateAsync/SoftDeleteAsync her çağrıda bu alanı günceller;
    //        "son işlemi kim yaptı" sorusu ActivityLog'a gitmeden doğrudan kayıttan cevaplanır.
    public int? UpdatedByUserId { get; set; }

    // AMAÇ: Kaydı soft delete eden kullanıcının Id'si. Silinmemişse null.
    // NEDEN: DeletedAt "ne zaman", bu alan "kim" sorusunu cevaplar;
    //        Repository<T>.SoftDeleteAsync ikisini birlikte set eder.
    public int? DeletedByUserId { get; set; }
}
