/// <summary>
/// ClassCategory.cs
///
/// AMAÇ:
///   Class (sınıf) ile Category (sistem kategorisi) arasındaki M:N ilişkiyi yönetir.
///   Öğretmen sınıfına "Aile" ve "Renkler" kategorilerini atayabilir.
///
/// BAĞIMLILIKLAR:
///   - Class (N:1)
///   - Category (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sınıf ↔ Sistem Kategorisi M:N ara tablo entity'si.
///
/// AMAÇ: Bir sınıfa hangi sistem kategorilerinin atandığını saklamak.
/// NEDEN BaseEntity'den miras almaz: Ara tablonun soft delete yoktur.
/// </summary>
public class ClassCategory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Sınıf ID'si (FK → Classes)</summary>
    public int ClassId { get; set; }

    /// <summary>Sistem kategorisi ID'si (FK → Categories)</summary>
    public int CategoryId { get; set; }

    /// <summary>Kategori gösterim sırası (sınıf içinde)</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>İlişkinin oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı sınıf (N:1)</summary>
    public Class Class { get; set; } = null!;

    /// <summary>Bağlı sistem kategorisi (N:1)</summary>
    public Category Category { get; set; } = null!;
}
