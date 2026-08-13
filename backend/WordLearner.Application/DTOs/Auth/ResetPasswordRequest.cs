namespace WordLearner.Application.DTOs.Auth;

public record ResetPasswordRequest(string Email, string OtpCode, string NewPassword);
