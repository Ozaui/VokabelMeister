using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.Srs;

// Sistem kelimesi (Word) ilerlemesi — bir kullanıcı+kelime çifti için TEK satır, SM-2 durumu burada
// tutulur (hesaplama SrsCalculator'da yapılır, bu entity yalnızca sonucu saklar).
public class UserProgress : BaseEntity
{
    public int UserId { get; set; }
    public int WordId { get; set; }

    public int CurrentLevel { get; set; }
    public decimal Mastery { get; set; }
    public decimal EasinessFactor { get; set; } = 2.5m;

    public int TimesCorrect { get; set; }
    public int TimesIncorrect { get; set; }
    public int TotalAttempts { get; set; }
    public decimal SuccessRate { get; set; }

    public DateTime? LastReviewedAt { get; set; }
    // NULL = hiç zamanlanmadı, yeni kelime havuzunda — SM-2 ilk tekrarı henüz hesaplanmadı.
    public DateTime? NextReviewAt { get; set; }

    public int IntervalDays { get; set; } = 1;
    public int RepetitionNumber { get; set; }

    // Leech tespiti — quality>=3 olan her doğru cevapta 0'a döner.
    public int ConsecutiveIncorrect { get; set; }
    public bool IsSuspended { get; set; }
}
