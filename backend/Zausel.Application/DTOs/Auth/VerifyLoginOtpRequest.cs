namespace Zausel.Application.DTOs.Auth;

public record VerifyLoginOtpRequest(string Email, string OtpCode);
