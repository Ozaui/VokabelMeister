/// <summary>
/// BaseEntity.cs
///
/// AMAÇ:
///   Tüm domain entity'lerin miras aldığı temel sınıf.
///   Ortak alanları (Id, zaman damgaları, soft delete) tek yerde tanımlar.
///
/// NEDEN:
///   DRY (Don't Repeat Yourself) prensibi — her entity'ye ayrı ayrı Id,
///   CreatedAt, UpdatedAt, IsDeleted yazmak yerine tek kaynaktan miras alınır.
///   EF Core global query filter ve SaveChangesAsync override bu sınıfa göre çalışır.
///
/// BAĞIMLILIKLAR:
///   Yok — Domain katmanının bağımsız temel sınıfı.
/// </summary>
namespace WordLearner.Domain.Common;

/// <summary>
/// Tüm entity'lerin miras aldığı soyut temel sınıf.
///
/// AMAÇ: Ortak alanları merkezi olarak yönetmek.
/// NEDEN: Her tabloda tekrarlanan alanları (Id, timestamp, soft delete) standartlaştırmak.
/// NASIL: Entity sınıfları bu sınıftan miras alır — EF Core ilişkilerini otomatik tanır.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Tablonun birincil anahtarı — otomatik artan (IDENTITY)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kaydın oluşturulma tarihi (UTC).
    /// NEDEN UTC: Farklı zaman dilimlerinde tutarsızlık oluşmasın.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Kaydın son güncellenme tarihi (UTC).
    /// NASIL: DbContext.SaveChangesAsync() override'ında otomatik güncellenir.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete bayrağı — true ise kayıt silinmiş sayılır ama veritabanında durur.
    /// NEDEN: Veri kaybını önlemek ve audit trail sağlamak için fiziksel silme yapılmaz.
    /// NASIL: DbContext global query filter ile IsDeleted=true kayıtlar sorgudan dışlanır.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Soft delete yapılma tarihi (UTC). IsDeleted=false iken NULL kalır.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
