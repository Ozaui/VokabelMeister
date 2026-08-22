using Zausel.Domain.Enums.Srs;

namespace Zausel.Domain.Entities.Srs;

// BaseEntity'den BİLİNÇLİ olarak türemez — DATABASE_SCHEMA/SRS.md'deki LearningSessions tablosu
// audit/soft-delete alanlarını hiç taşımıyor (Achievement/LearningHistory ile AYNI gerekçe): bir
// oturum kimse tarafından silinmez/düzenlenmez, yalnızca sahibi tarafından tamamlanır/bırakılır.
public class LearningSession
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Hangi yönde öğreniliyor (de→tr mi tr→de mi) — bu dilin Words/WordDetail'i "ön yüz" (gramer
    // test edilir), diğer dilin Words.Text/Definition yalnızca çeviri olarak gösterilir.
    public int TargetLanguageId { get; set; }

    public LearningSessionType SessionType { get; set; }
    public LearningSessionSourceType SourceType { get; set; }

    public string? LevelFilter { get; set; }
    // JSON [1,3,5] — filtre seçimleri, ayrı bir ara tabloya çıkarılmaz (sorgu değil, yalnızca
    // oturumun nasıl başlatıldığının kaydı).
    public string? CategoryIds { get; set; }
    public string? UserCategoryIds { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DurationSeconds { get; set; }

    public int TotalWords { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public decimal? SuccessRate { get; set; }
    public int XPEarned { get; set; }

    public LearningSessionStatus Status { get; set; } = LearningSessionStatus.Active;
}
