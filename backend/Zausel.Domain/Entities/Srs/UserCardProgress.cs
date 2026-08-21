using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.Srs;

// Kişisel kart (UserCard) ilerlemesi — UserProgress ile birebir aynı şema, yalnızca WordId yerine
// UserCardId taşır; sistem kelimesi ile kullanıcının kendi kartı ayrı havuzlarda ilerlediği için
// ayrı tablo (bkz. DATABASE_SCHEMA/SRS.md).
public class UserCardProgress : BaseEntity
{
    public int UserId { get; set; }
    public int UserCardId { get; set; }

    public int CurrentLevel { get; set; }
    public decimal Mastery { get; set; }
    public decimal EasinessFactor { get; set; } = 2.5m;

    public int TimesCorrect { get; set; }
    public int TimesIncorrect { get; set; }
    public int TotalAttempts { get; set; }
    public decimal SuccessRate { get; set; }

    public DateTime? LastReviewedAt { get; set; }
    public DateTime? NextReviewAt { get; set; }

    public int IntervalDays { get; set; } = 1;
    public int RepetitionNumber { get; set; }

    public int ConsecutiveIncorrect { get; set; }
    public bool IsSuspended { get; set; }
}
