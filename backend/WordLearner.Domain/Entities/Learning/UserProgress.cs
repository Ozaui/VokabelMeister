/// <summary>
/// UserProgress.cs
///
/// AMAÇ:
///   Bir kullanıcının belirli bir sistem kelimesiyle (Word) ilgili
///   öğrenme ilerlemesini ve SRS (Spaced Repetition System) durumunu saklar.
///
/// NEDEN:
///   Her kullanıcı × kelime çifti için ayrı ilerleme kaydı tutulur.
///   SM-2 algoritması bu verileri kullanarak bir sonraki tekrar tarihini hesaplar.
///   Sistem kelimeleri için ayrı, kişisel kartlar için ayrı tablo (UserCardProgress) kullanılır.
///
/// SM-2 ALGORİTMASI:
///   Level 0 → 1 gün, Level 1 → 3 gün, Level 2 → 7 gün,
///   Level 3 → 14 gün, Level 4 → 30 gün, Level 5 → 60 gün (mastery)
///   Yanlış cevap → Level 0'a reset
///
/// BAĞIMLILIKLAR:
///   - User (N:1 — kimin ilerlemesi)
///   - Word (N:1 — hangi kelime)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sistem kelimesi SRS ilerleme entity'si.
///
/// AMAÇ: Kullanıcının sistem kelimelerindeki öğrenme durumunu SM-2 algoritmasıyla takip etmek.
/// NEDEN BaseEntity'den miras almaz: Soft delete gerekmez; kullanıcı silinince CASCADE ile silinir.
/// </summary>
public class UserProgress
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>Kelime ID'si (FK → Words)</summary>
    public int WordId { get; set; }

    // ─── Öğrenme Seviyesi ────────────────────────────────────────────────────

    /// <summary>
    /// SM-2 öğrenme seviyesi (0–5).
    /// 0 = Hiç görülmedi / yeni başlandı
    /// 3 = Orta düzey hatırlama
    /// 5 = Otomatik hatırlama (mastery)
    /// </summary>
    public int CurrentLevel { get; set; } = 0;

    /// <summary>
    /// Mastery yüzdesi (0.00–100.00).
    /// NEDEN: İlerleme çubuğu ve kullanıcı istatistikleri için görsel gösterge.
    /// </summary>
    public decimal Mastery { get; set; } = 0;

    // ─── İstatistikler ────────────────────────────────────────────────────────

    /// <summary>Toplam doğru cevap sayısı</summary>
    public int TimesCorrect { get; set; } = 0;

    /// <summary>Toplam yanlış cevap sayısı</summary>
    public int TimesIncorrect { get; set; } = 0;

    /// <summary>Toplam girişim sayısı (TimesCorrect + TimesIncorrect)</summary>
    public int TotalAttempts { get; set; } = 0;

    /// <summary>
    /// Başarı oranı yüzdesi (0.00–100.00).
    /// HESAPLAMA: (TimesCorrect / TotalAttempts) * 100
    /// </summary>
    public decimal SuccessRate { get; set; } = 0;

    // ─── SRS Zamanlaması ──────────────────────────────────────────────────────

    /// <summary>Son tekrar tarihi (UTC). NULL ise hiç çalışılmamış.</summary>
    public DateTime? LastReviewedAt { get; set; }

    /// <summary>
    /// Bir sonraki önerilen tekrar tarihi (UTC).
    /// VARSAYILAN: Oluşturulma tarihi — yeni kelime hemen çalışılabilir.
    /// NASIL GÜNCELLENİR: SM-2 algoritması her cevaptan sonra yeniden hesaplar.
    /// </summary>
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mevcut SRS aralığı (gün cinsinden).
    /// ÖRNEK: Level 2 → 7 gün; bir sonraki tekrar 7 gün sonra.
    /// </summary>
    public int IntervalDays { get; set; } = 1;

    /// <summary>
    /// SM-2 tekrar sayacı — birbirini izleyen başarılı tekrar sayısı.
    /// NEDEN: SM-2 formülünde I(n) = I(n-1) * EF hesaplaması için gerekli.
    /// </summary>
    public int RepetitionNumber { get; set; } = 0;

    /// <summary>
    /// SM-2 Easiness Factor (kolaylık çarpanı) — varsayılan 2.5.
    /// NEDEN: Kullanıcının öz değerlendirmesine (SelfRating) göre güncellenir.
    ///        Yüksek EF → interval daha hızlı büyür (kelime kolay hatırlanıyor).
    ///        Düşük EF  → interval yavaş büyür (kelime zor hatırlanıyor).
    ///
    /// SM-2 FORMÜLÜ:
    ///   EF(yeni) = EF(eski) + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
    ///   q = SelfRating (0–5)
    ///   Minimum EF = 1.3 (hiç sıfırlanmaz)
    /// </summary>
    public decimal EasinessFactor { get; set; } = 2.5m;

    /// <summary>Kayıt oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>İlerlemenin sahibi kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>İlerlemenin ait olduğu kelime (N:1)</summary>
    public Word Word { get; set; } = null!;
}
