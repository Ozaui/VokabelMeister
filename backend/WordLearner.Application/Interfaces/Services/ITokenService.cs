using System.Security.Claims;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Services;

public record RefreshTokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(User user);

    // JWT DEĞİL — düz rastgele veri, kendisi kimlik taşımaz, yalnızca DB'deki RefreshTokens kaydıyla eşleşen bir sırdır.
    RefreshTokenResult GenerateRefreshToken();

    // ValidateLifetime=false ile süre kontrolü bilerek atlanır, ama imza + algoritma kontrolü atlanmaz.
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
