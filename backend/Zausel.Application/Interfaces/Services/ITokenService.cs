using System.Security.Claims;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Application.Interfaces.Services;

public record RefreshTokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshTokenResult GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
