/// <summary>
/// UserAchievement.cs
///
/// AMAÇ:
///   Kullanıcı × Achievement M:N ilişkisini yönetir.
///   Bir kullanıcı bir rozeti yalnızca bir kez kazanabilir (UNIQUE kısıtı).
///
/// NEDEN:
///   Achievement tanımları (Achievement tablosu) sabit kalır.
///   Hangi kullanıcının ne zaman hangi rozeti kazandığı bu ara tabloda saklanır.
///
/// BAĞIMLILIKLAR:
///   - User (N:1)
///   - Achievement (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kullanıcı rozet kazanımı entity'si (User ↔ Achievement M:N ara tablo).
///
/// AMAÇ: Rozet kazanım olayını kullanıcı ve tarihiyle birlikte kayıt altına almak.
/// NEDEN BaseEntity'den miras almaz: Soft delete yoktur; rozet geri alınamaz.
/// </summary>
public class UserAchievement
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Rozeti kazanan kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>Kazanılan rozet ID'si (FK → Achievements)</summary>
    public int AchievementId { get; set; }

    /// <summary>Rozeti kazanma tarihi (UTC)</summary>
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Rozeti kazanan kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>Kazanılan rozet tanımı (N:1)</summary>
    public Achievement Achievement { get; set; } = null!;
}
