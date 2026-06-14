/// <summary>
/// ClassUserCategory.cs
///
/// AMAÇ:
///   Class (sınıf) ile UserCategory (kişisel kategori) arasındaki M:N ilişkiyi yönetir.
///   Öğretmen kendi oluşturduğu kişisel kategorisini sınıfıyla paylaşabilir.
///
/// BAĞIMLILIKLAR:
///   - Class (N:1)
///   - UserCategory (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sınıf ↔ Kişisel Kategori M:N ara tablo entity'si.
///
/// AMAÇ: Bir sınıfa hangi kişisel kategorilerin atandığını saklamak.
/// NEDEN BaseEntity'den miras almaz: Ara tablonun soft delete yoktur.
/// </summary>
public class ClassUserCategory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Sınıf ID'si (FK → Classes)</summary>
    public int ClassId { get; set; }

    /// <summary>Kişisel kategori ID'si (FK → UserCategories)</summary>
    public int UserCategoryId { get; set; }

    /// <summary>Kategori gösterim sırası (sınıf içinde)</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>İlişkinin oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı sınıf (N:1)</summary>
    public Class Class { get; set; } = null!;

    /// <summary>Bağlı kişisel kategori (N:1)</summary>
    public UserCategory UserCategory { get; set; } = null!;
}
