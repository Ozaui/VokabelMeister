using Zausel.Domain.Entities.Auth;

namespace Zausel.Application.Interfaces.Services;

// RefreshTokenEntity henüz PERSIST EDİLMEMİŞ — çağıran Handler kendi repository'siyle kaydeder
// (OtpService ile aynı desen: servis User'ı mutasyona uğratır, DB yazımı Handler'ın sorumluluğu).
public record LoginCompletionResult(
    string AccessToken,
    string RefreshTokenValue,
    RefreshToken RefreshTokenEntity,
    bool AccountWasRecovered);

// OTP/Google/Apple/QR girişlerinin ortak son adımı: hesap durumu kontrolü (anonimleştirilmiş →
// reddet, grace period içindeyse kurtar) + access/refresh token üretimi.
public interface ILoginCompletionService
{
    LoginCompletionResult Complete(User user, string? deviceInfo, string? ipAddress);
}
