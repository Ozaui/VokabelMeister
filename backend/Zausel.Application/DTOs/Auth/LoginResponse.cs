namespace Zausel.Application.DTOs.Auth;

// VerifyLoginOtp/LoginWithGoogle/LoginWithApple ortak yanıt şekli — hepsi aynı ILoginCompletionService
// akışından geçer (SECURITY.md §1.3).
public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, AuthUserDto User, bool AccountWasRecovered);
