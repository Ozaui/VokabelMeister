/// <summary>
/// UserCardUserCategory.cs
///
/// AMAÇ:
///   UserCard (kişisel kart) ile UserCategory (kişisel kategori) arasındaki M:N ilişkiyi yönetir.
///
/// NEDEN:
///   Kullanıcılar kendi kartlarını kendi oluşturdukları kategorilerle de gruplayabilir.
///   UserCardCategory'den farklı olarak burada her iki taraf da kullanıcıya aittir.
///
/// GÜVENLİK NOTU:
///   UserCard.UserId ve UserCategory.UserId'nin eşleştiği kontrol edilmeli —
///   kullanıcı başkasının kategorisine kendi kartını bağlayamamalı.
///
/// BAĞIMLILIKLAR:
///   - UserCard (N:1)
///   - UserCategory (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// UserCard ↔ UserCategory M:N ara tablo entity'si.
///
/// AMAÇ: Kişisel kartların kişisel kategorilerle bağlantısını saklamak.
/// NEDEN BaseEntity'den miras almaz: Ara tablonun soft delete yoktur.
/// </summary>
public class UserCardUserCategory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kişisel kart ID'si (FK → UserCards)</summary>
    public int UserCardId { get; set; }

    /// <summary>Kişisel kategori ID'si (FK → UserCategories)</summary>
    public int UserCategoryId { get; set; }

    /// <summary>İlişkinin oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı kişisel kart (N:1)</summary>
    public UserCard UserCard { get; set; } = null!;

    /// <summary>Bağlı kişisel kategori (N:1)</summary>
    public UserCategory UserCategory { get; set; } = null!;
}
