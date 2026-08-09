using System.Security.Claims;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Services;

public record RefreshTokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshTokenResult GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
