using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

public record RefreshCommand(string RefreshToken, string? DeviceInfo, string? IpAddress) : IRequest<RefreshResponse>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResponse>
{
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly ISecurityLogger _securityLogger;

    public RefreshCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        ISecurityLogger securityLogger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _securityLogger = securityLogger;
    }

    public async Task<RefreshResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (existingToken.IsUsed)
        {
            // Token Family Pattern: daha önce kullanılmış bir refresh token TEKRAR geldi — çalınmış olabilir.
            await _securityLogger.LogAsync(LogEventType.TokenReplay, userId: existingToken.UserId,
                ipAddress: request.IpAddress, userAgent: request.DeviceInfo, detail: "INVALID_REFRESH_TOKEN",
                cancellationToken: cancellationToken);
            await _refreshTokenRepository.RevokeFamilyAsync(existingToken.TokenFamily, cancellationToken);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
            throw new InvalidRefreshTokenException();
        }

        if (existingToken.RevokedAt is not null || existingToken.ExpiresAt < DateTime.UtcNow)
            throw new InvalidRefreshTokenException();

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted || user.IsAnonymized)
            throw new InvalidRefreshTokenException();

        existingToken.IsUsed = true;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefresh = _tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(newRefresh.Token),
            TokenFamily = existingToken.TokenFamily,
            ExpiresAt = newRefresh.ExpiresAt,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new RefreshResponse(accessToken, newRefresh.Token, AccessTokenExpiresInSeconds);
    }
}
