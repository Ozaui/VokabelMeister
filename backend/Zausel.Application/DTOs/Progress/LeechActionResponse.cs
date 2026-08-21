namespace Zausel.Application.DTOs.Progress;

// Şekil aksiyona göre değişir — yalnızca ilgili alan(lar) dolar: Suspend → IsSuspended,
// Reset → CurrentLevel+NextReviewAt, Continue → Acknowledged (hiçbir alan MUTLAKA birlikte dolmaz).
public record LeechActionResponse(bool? IsSuspended, int? CurrentLevel, DateTime? NextReviewAt, bool? Acknowledged);
