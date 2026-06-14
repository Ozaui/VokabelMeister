/// <summary>
/// LearningSession.cs
///
/// AMAÇ:
///   Bir öğrenme oturumunun özet bilgilerini saklar.
///   Kullanıcının "Çalış" butonuna basmasından oturumu tamamlamasına kadar geçen süreci kaydeder.
///
/// NEDEN:
///   Birden fazla LearningHistory kaydını bir oturum altında gruplamak,
///   "Bu haftaki çalışmalarım" gibi özet istatistikler sunmayı kolaylaştırır.
///   CategoryIds JSON olarak saklanır çünkü kullanıcı aynı anda birden fazla kategori seçebilir.
///
/// BAĞIMLILIKLAR:
///   - User (N:1 — oturumu başlatan kullanıcı)
///   - LearningHistory (1:N — oturumda yapılan girişimler)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Öğrenme oturumu özet entity'si.
///
/// AMAÇ: Bir öğrenme seansının başlangıç, bitiş ve sonuç bilgilerini kayıt altına almak.
/// NEDEN BaseEntity'den miras almaz: Soft delete gerekmez; kullanıcı silinince CASCADE ile silinir.
/// </summary>
public class LearningSession
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Oturumu başlatan kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    // ─── Oturum Kapsamı ──────────────────────────────────────────────────────

    /// <summary>
    /// Oturum türü: Flashcard | MultipleChoice | ArticleQuiz | PluralQuiz | TranslationQuiz
    /// </summary>
    public string SessionType { get; set; } = string.Empty;

    /// <summary>
    /// İçerik kaynağı: SystemWords | UserCards | Mixed
    /// NEDEN: Kullanıcı "sadece kişisel kartlarımla çalış" diyebilir.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// CEFR seviye filtresi (NULL = tüm seviyeler).
    /// ÖRNEK: "A1" seçilirse sadece A1 kelimeleri çalışılır.
    /// </summary>
    public string? LevelFilter { get; set; }

    /// <summary>
    /// Seçilen sistem kategori ID'leri — JSON dizi formatında.
    /// ÖRNEK: "[1, 3, 5]"
    /// NEDEN JSON: Birden fazla kategori seçilebilir; ayrı tablo açmak overkill.
    /// </summary>
    public string? CategoryIds { get; set; }

    /// <summary>
    /// Seçilen kişisel kategori ID'leri — JSON dizi formatında.
    /// ÖRNEK: "[2, 7]"
    /// </summary>
    public string? UserCategoryIds { get; set; }

    // ─── Zaman Bilgisi ────────────────────────────────────────────────────────

    /// <summary>Oturumun başlama zamanı (UTC)</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Oturumun tamamlanma zamanı (UTC). NULL ise devam ediyor veya terk edildi.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Toplam oturum süresi (saniye). NULL ise tamamlanmamış.</summary>
    public int? DurationSeconds { get; set; }

    // ─── Sonuçlar ─────────────────────────────────────────────────────────────

    /// <summary>Oturumdaki toplam kelime/kart sayısı</summary>
    public int TotalWords { get; set; } = 0;

    /// <summary>Doğru cevap sayısı</summary>
    public int CorrectAnswers { get; set; } = 0;

    /// <summary>Yanlış cevap sayısı</summary>
    public int IncorrectAnswers { get; set; } = 0;

    /// <summary>Başarı oranı yüzdesi (NULL ise tamamlanmamış)</summary>
    public decimal? SuccessRate { get; set; }

    /// <summary>Bu oturumda kazanılan XP</summary>
    public int XPEarned { get; set; } = 0;

    /// <summary>
    /// Oturum durumu: Active | Completed | Abandoned
    /// Active    : Devam ediyor
    /// Completed : Başarıyla tamamlandı
    /// Abandoned : Kullanıcı yarıda bıraktı
    /// </summary>
    public string Status { get; set; } = "Active";

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Oturumu başlatan kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>Bu oturumda yapılan tüm girişimler (1:N)</summary>
    public ICollection<LearningHistory> LearningHistories { get; set; } = new List<LearningHistory>();
}
