namespace Zausel.Application.Interfaces.Repositories.Srs;

// GetProgressSummaryQuery'nin bant/due sayımı için ihtiyaç duyduğu üç alan — UserProgress VE
// UserCardProgress'in İKİSİNDEN de AYNI şekilde üretilir, iki tabloyu TEK bir sayım mantığında birleştirir.
public record ProgressSnapshot(decimal Mastery, DateTime? NextReviewAt, bool IsSuspended);
