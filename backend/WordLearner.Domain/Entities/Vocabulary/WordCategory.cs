/// <summary>
/// WordCategory.cs
///
/// AMAÇ:
///   Word (sistem kelimesi) ile Category arasındaki M:N ilişkiyi yöneten ara tablo.
///   Bir kelime birden fazla kategoriye, bir kategori birden fazla kelimeye ait olabilir.
///
/// NEDEN:
///   M:N ilişki doğrudan kurulamaz — ara tabloya ihtiyaç vardır.
///   DisplayOrder sayesinde kategori içinde kelimelerin sırası kontrol edilir.
///
/// BAĞIMLILIKLAR:
///   - Word (N:1)
///   - Category (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Word ↔ Category M:N ara tablo entity'si.
///
/// AMAÇ: Hangi kelimenin hangi kategorilere ait olduğunu saklamak.
/// NEDEN BaseEntity'den miras almaz: Ara tablonun soft delete veya UpdatedAt'ı yoktur.
/// </summary>
public class WordCategory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kelime ID'si (FK → Words)</summary>
    public int WordId { get; set; }

    /// <summary>Kategori ID'si (FK → Categories)</summary>
    public int CategoryId { get; set; }

    /// <summary>Kategori içindeki gösterim sırası</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>İlişkinin oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı kelime (N:1)</summary>
    public Word Word { get; set; } = null!;

    /// <summary>Bağlı kategori (N:1)</summary>
    public Category Category { get; set; } = null!;
}
