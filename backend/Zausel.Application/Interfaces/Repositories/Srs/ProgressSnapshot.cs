namespace Zausel.Application.Interfaces.Repositories.Srs;

// GetProgressSummaryQuery'nin bant/due sayımı VE AchievementService'in eşik değerlendirmesi için
// ihtiyaç duyduğu alanlar — UserProgress VE UserCardProgress'in İKİSİNDEN de AYNI şekilde üretilir,
// iki tabloyu TEK bir sayım/değerlendirme mantığında birleştirir. CurrentLevel yalnızca
// AchievementService kullanır (GetProgressSummaryQuery'nin ihtiyacı yok) — varsayılan değeriyle
// GetProgressSummaryQueryHandlerTests'teki 3-parametreli mevcut çağrılar bozulmadan kalır.
public record ProgressSnapshot(decimal Mastery, DateTime? NextReviewAt, bool IsSuspended, int CurrentLevel = 0);
