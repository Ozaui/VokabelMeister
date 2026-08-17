using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Application.Services;

public class LoginCompletionService : ILoginCompletionService
{
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public LoginCompletionService(ITokenService tokenService, IPasswordService passwordService)
    {
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public LoginCompletionResult Complete(User user, string? deviceInfo, string? ipAddress)
    {
        // IsAnonymized kontrolü grace-period kurtarmadan ÖNCE yapılır: aksi halde kalıcı silinmiş
        // (PII temizlenmiş) bir hesap da IsDeleted=true olduğu için yanlışlıkla geri aktifleştirilir.
        if (user.IsAnonymized)
            throw new AccountAnonymizedException();

        var accountWasRecovered = user.IsDeleted;
        if (accountWasRecovered)
        {
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.ScheduledDeletionAt = null;
            user.IsActive = true;
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIP = ipAddress;
        user.LoginCount++;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refresh = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(refresh.Token),
            TokenFamily = Guid.NewGuid().ToString(),
            ExpiresAt = refresh.ExpiresAt,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        return new LoginCompletionResult(accessToken, refresh.Token, refreshTokenEntity, accountWasRecovered);
    }
}
