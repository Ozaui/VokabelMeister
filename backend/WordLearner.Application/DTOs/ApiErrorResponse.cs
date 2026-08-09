namespace WordLearner.Application.DTOs;

public record ApiErrorResponse(bool Success, ApiErrorDetail Error);

public record ApiErrorDetail(string Code, string Message);
