/// <summary>
/// UserCardProgress.cs
///
/// AMAÇ:
///   Bir kullanıcının kişisel kartlarındaki (UserCard) öğrenme ilerlemesini
///   ve SRS durumunu saklar. UserProgress ile aynı yapıdadır fakat kişisel kartlar içindir.
///
/// NEDEN AYRI TABLO:
///   Sistem kelimeleri (UserProgress) ve kişisel kartlar (UserCardProgress) farklı
///   gruplarda çalışılabilir — kullanıcı "Sadece kişisel kartlarımı çalış" diyebilir.
///   İki tabloyu birleştirmek WHERE koşullarını karmaşıklaştırır.
///
/// BAĞIMLILIKLAR:
///   - User (N:1 — kimin ilerlemesi)
///   - UserCard (N:1 — hangi kişisel kart)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kişisel kart SRS ilerleme entity'si.
///
/// AMAÇ: Kullanıcının kişisel kartlarındaki öğrenme durumunu SM-2 algoritmasıyla takip etmek.
/// NEDEN BaseEntity'den miras almaz: UserCard silinince CASCADE ile silinir.
/// </summary>
public class UserCardProgress
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>Kişisel kart ID'si (FK → UserCards)</summary>
    public int UserCardId { get; set; }

    // ─── Öğrenme Seviyesi ────────────────────────────────────────────────────

    /// <summary>SM-2 öğrenme seviyesi (0–5). Bkz. UserProgress.CurrentLevel açıklaması.</summary>
    public int CurrentLevel { get; set; } = 0;

    /// <summary>Mastery yüzdesi (0.00–100.00)</summary>
    public decimal Mastery { get; set; } = 0;

    // ─── İstatistikler ────────────────────────────────────────────────────────

    /// <summary>Toplam doğru cevap sayısı</summary>
    public int TimesCorrect { get; set; } = 0;

    /// <summary>Toplam yanlış cevap sayısı</summary>
    public int TimesIncorrect { get; set; } = 0;

    /// <summary>Toplam girişim sayısı</summary>
    public int TotalAttempts { get; set; } = 0;

    /// <summary>Başarı oranı yüzdesi (0.00–100.00)</summary>
    public decimal SuccessRate { get; set; } = 0;

    // ─── SRS Zamanlaması ──────────────────────────────────────────────────────

    /// <summary>Son tekrar tarihi (UTC)</summary>
    public DateTime? LastReviewedAt { get; set; }

    /// <summary>Bir sonraki önerilen tekrar tarihi (UTC)</summary>
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;

    /// <summary>Mevcut SRS aralığı (gün cinsinden)</summary>
    public int IntervalDays { get; set; } = 1;

    /// <summary>SM-2 tekrar sayacı — birbirini izleyen başarılı tekrar sayısı</summary>
    public int RepetitionNumber { get; set; } = 0;

    /// <summary>
    /// SM-2 Easiness Factor (kolaylık çarpanı) — varsayılan 2.5.
    /// UserProgress.EasinessFactor ile aynı mantık, kişisel kartlar için.
    /// Bkz. UserProgress.EasinessFactor açıklaması.
    /// </summary>
    public decimal EasinessFactor { get; set; } = 2.5m;

    /// <summary>Kayıt oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>İlerlemenin sahibi kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>İlerlemenin ait olduğu kişisel kart (N:1)</summary>
    public UserCard UserCard { get; set; } = null!;
}
