namespace WordLearner.Application.DTOs.Auth;

public record VerifyEmailRequest(string Email, string OtpCode);
