namespace Zausel.Application.DTOs.Auth;

public record ResetPasswordRequest(string Email, string OtpCode, string NewPassword);
