namespace Zausel.Application.DTOs.Auth;

public record VerifyEmailRequest(string Email, string OtpCode);
