namespace Zausel.Application.DTOs.Progress;

public record ProgressWordResponse(
    int WordId, string Text, string? Definition, int CurrentLevel, decimal Mastery, DateTime? NextReviewAt, bool IsSuspended);
