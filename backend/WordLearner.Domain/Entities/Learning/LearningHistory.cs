/// <summary>
/// LearningHistory.cs
///
/// AMAÇ:
///   Kullanıcının her öğrenme girişimini (cevap) kalıcı olarak kaydeder.
///   Hem sistem kelimeleri hem kişisel kartlar için ortak kayıt tablosudur.
///
/// NEDEN:
///   Audit trail: Kim, ne zaman, hangi kelimeye, doğru mu yanlış mı cevap verdi?
///   Analitik: Hangi kelimeler sorun çıkarıyor? Hangi saat diliminde daha başarılı?
///   SRS hesaplama: Geçmişe dayalı adaptif öğrenme için ham veri.
///
/// ÖNEMLI KISIT (DATABASE_SCHEMA.md §4):
///   WordId ve UserCardId'den biri dolu OLMALIDLIR — ikisi birden NULL olamaz.
///   Bu kontrol uygulama katmanında (service) yapılır.
///
/// BAĞIMLILIKLAR:
///   - User (N:1)
///   - Word (N:1, opsiyonel — sistem kelimesi girişimlerinde dolu)
///   - UserCard (N:1, opsiyonel — kişisel kart girişimlerinde dolu)
///   - LearningSession (N:1, opsiyonel — bir oturum içindeyse)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Öğrenme girişimi tarihçe kaydı entity'si.
///
/// AMAÇ: Her cevap girişimini kalıcı olarak kaydetmek (append-only).
/// NEDEN BaseEntity'den miras almaz: Tarihçe kaydı hiç silinmez, UpdatedAt yoktur.
/// </summary>
public class LearningHistory
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Cevabı veren kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Sistem kelimesi ID'si (FK → Words, opsiyonel).
    /// KURAL: UserCardId NULL ise bu dolu OLMALIDIR.
    /// </summary>
    public int? WordId { get; set; }

    /// <summary>
    /// Kişisel kart ID'si (FK → UserCards, opsiyonel).
    /// KURAL: WordId NULL ise bu dolu OLMALIDIR.
    /// </summary>
    public int? UserCardId { get; set; }

    /// <summary>Bu girişimin yapıldığı öğrenme oturumu (FK → LearningSessions, opsiyonel)</summary>
    public int? LearningSessionId { get; set; }

    /// <summary>
    /// Oturum türü: Flashcard | MultipleChoice | ArticleQuiz | PluralQuiz | TranslationQuiz
    /// NEDEN: Türe göre istatistik analizi yapılabilir.
    /// </summary>
    public string? SessionType { get; set; }

    /// <summary>Cevap doğru muydu?</summary>
    public bool IsCorrect { get; set; }

    /// <summary>Cevap süresi (milisaniye cinsinden)</summary>
    public int? ResponseTime { get; set; }

    /// <summary>Bu soru için harcanan süre (saniye)</summary>
    public int? TimeSpentSeconds { get; set; }

    /// <summary>Kullanıcının verdiği cevap metni (max 500 karakter)</summary>
    public string? UserResponse { get; set; }

    /// <summary>Doğru cevap metni (max 500 karakter)</summary>
    public string? CorrectResponse { get; set; }

    /// <summary>
    /// Kullanıcının öz değerlendirmesi (0–5, SM-2 quality değeri).
    /// NULL ise kullanıcı değerlendirme yapmadı (eski kayıtlar için).
    ///
    /// UI'da 4 buton olarak gösterilir:
    ///   🔴 Bilmedim    → 0 (Level sıfırlanır)
    ///   🟠 Zor         → 2 (Interval kısalır)
    ///   🟢 İyi         → 4 (Normal ilerleme)
    ///   🔵 Çok Kolay   → 5 (Interval uzar)
    ///
    /// NEDEN: IsCorrect (bool) yetersizdir — "doğru ama zorlandım" ile
    ///        "anında hatırladım" aynı şeyi ifade etmez.
    ///        SM-2 algoritması quality değerine göre EasinessFactor'ı günceller.
    /// </summary>
    public int? SelfRating { get; set; }

    /// <summary>Kayıt oluşturulma tarihi (UTC) — girişim zamanı</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Girişimi yapan kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>Çalışılan sistem kelimesi (N:1, opsiyonel)</summary>
    public Word? Word { get; set; }

    /// <summary>Çalışılan kişisel kart (N:1, opsiyonel)</summary>
    public UserCard? UserCard { get; set; }

    /// <summary>Bu girişimin ait olduğu öğrenme oturumu (N:1, opsiyonel)</summary>
    public LearningSession? LearningSession { get; set; }
}
