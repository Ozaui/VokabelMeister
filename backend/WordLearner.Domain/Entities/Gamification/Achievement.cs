/// <summary>
/// Achievement.cs
///
/// AMAÇ:
///   Kullanıcıların kazanabileceği başarı rozetlerini (achievement) tanımlar.
///   Admin tarafından yönetilir — sistem genelinde standarttır.
///
/// NEDEN:
///   Gamification: Belirli hedeflere ulaşıldığında ödül vermek kullanıcı motivasyonunu artırır.
///   Örn: "10 gün streak", "100 kelime öğrenildi", "B1 seviyesine ulaşıldı"
///
/// BAĞIMLILIKLAR:
///   - UserAchievement (1:N — hangi kullanıcılar bu rozeti kazandı)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Başarı rozeti tanımı entity'si.
///
/// AMAÇ: Kullanıcılara verilebilecek rozetlerin katalogunu saklamak.
/// NEDEN BaseEntity'den miras almaz: Soft delete gerekmez; silme nadirdir ve manuel yapılır.
/// </summary>
public class Achievement
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Rozet adı (max 100 karakter). ÖRNEK: "İlk Kelime", "Streak Ustası"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Rozet açıklaması (max 500 karakter). Nasıl kazanılır?</summary>
    public string? Description { get; set; }

    /// <summary>Rozet ikonu URL'si veya ikon adı</summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Bu rozeti kazanmak için verilen XP ödülü.
    /// NEDEN: Çaba gerektiren rozetler daha fazla XP verir.
    /// </summary>
    public int RewardXP { get; set; } = 0;

    /// <summary>
    /// Rozet nadirlik derecesi: Common | Rare | Epic | Legendary
    /// Common    : Herkesin kolayca kazanabileceği
    /// Rare      : Biraz çaba gerektiren
    /// Epic      : Uzun vadeli hedef
    /// Legendary : Çok zor, az kişide var
    /// </summary>
    public string Rarity { get; set; } = "Common";

    /// <summary>Kayıt oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bu rozeti kazanan kullanıcılar (1:N)</summary>
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
