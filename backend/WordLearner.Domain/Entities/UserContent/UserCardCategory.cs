/// <summary>
/// UserCardCategory.cs
///
/// AMAÇ:
///   UserCard (kişisel kart) ile Category (sistem kategorisi) arasındaki M:N ilişkiyi yönetir.
///
/// NEDEN:
///   Kullanıcılar kişisel kartlarını sistem kategorileriyle etiketleyebilir.
///   Bu sayede admin onaylı kategorilerde arama ve filtreleme yapılabilir.
///
/// BAĞIMLILIKLAR:
///   - UserCard (N:1)
///   - Category (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// UserCard ↔ Category M:N ara tablo entity'si.
///
/// AMAÇ: Kişisel kartların sistem kategorileriyle bağlantısını saklamak.
/// NEDEN BaseEntity'den miras almaz: Ara tablonun soft delete yoktur.
/// </summary>
public class UserCardCategory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kişisel kart ID'si (FK → UserCards)</summary>
    public int UserCardId { get; set; }

    /// <summary>Sistem kategorisi ID'si (FK → Categories)</summary>
    public int CategoryId { get; set; }

    /// <summary>İlişkinin oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı kişisel kart (N:1)</summary>
    public UserCard UserCard { get; set; } = null!;

    /// <summary>Bağlı sistem kategorisi (N:1)</summary>
    public Category Category { get; set; } = null!;
}
