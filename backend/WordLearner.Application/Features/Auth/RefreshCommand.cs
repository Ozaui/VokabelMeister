using AutoMapper;
using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<AuthTokenResponse>
{
    public string? ClientIp { get; init; }
}

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly IMapper _mapper;
    private readonly ISecurityLogger _securityLogger;

    public RefreshCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        ILoginCompletionService loginCompletionService,
        IMapper mapper,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _loginCompletionService = loginCompletionService;
        _mapper = mapper;
        _securityLogger = securityLogger;
    }

    public async Task<AuthTokenResponse> Handle(RefreshCommand request, CancellationToken ct)
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (
            existingToken is null
            || existingToken.RevokedAt is not null
            || existingToken.ExpiresAt < DateTime.UtcNow
        )
            throw new InvalidRefreshTokenException();

        if (existingToken.IsUsed)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(existingToken.TokenFamily, ct);
            // Detail bir Code, serbest metin değil — admin GET /admin/logs/security ile
            // OKURKEN kendi Accept-Language'ıyla çözülür (CLAUDE.md §1 "İkinci istisna").
            await _securityLogger.LogAsync(
                LogEventType.TokenReplay,
                existingToken.UserId,
                ipAddress: request.ClientIp,
                detail: "TOKEN_REPLAY_FAMILY_REVOKED",
                ct: ct
            );
            throw new InvalidRefreshTokenException();
        }

        existingToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(existingToken, existingToken.UserId, ct);

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, ct);
        if (user is null || !user.IsActive || user.IsAnonymized)
            throw new InvalidRefreshTokenException();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenResult = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(refreshTokenResult.Token),
            TokenFamily = existingToken.TokenFamily,
            ExpiresAt = refreshTokenResult.ExpiresAt,
            DeviceInfo = existingToken.DeviceInfo,
            IpAddress = request.ClientIp,
        };
        await _refreshTokenRepository.AddAsync(newRefreshToken, user.Id, ct);

        return new AuthTokenResponse(
            accessToken,
            refreshTokenResult.Token,
            _loginCompletionService.ExpiresInSeconds(),
            _mapper.Map<AuthUserDto>(user),
            false
        );
    }
}
