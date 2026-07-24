using AutoMapper;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Services;

public class LoginCompletionService : ILoginCompletionService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public LoginCompletionService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOtpService otpService,
        IConfiguration configuration,
        IMapper mapper
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _otpService = otpService;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<AuthTokenResponse> CompleteLoginAsync(
        User user,
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        if (user.IsAnonymized)
            throw new AccountAnonymizedException();

        var accountWasRecovered = false;
        if (user.IsDeleted)
        {
            // 30 günlük grace period içinde soft-delete'li bir hesap otomatik kurtarılır.
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.ScheduledDeletionAt = null;
            accountWasRecovered = true;
        }

        _otpService.Clear(user);
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIP = ipAddress;
        user.LoginCount += 1;
        await _userRepository.UpdateAsync(user, user.Id, ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenResult = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(refreshTokenResult.Token),
            TokenFamily = Guid.NewGuid().ToString(),
            ExpiresAt = refreshTokenResult.ExpiresAt,
            IpAddress = ipAddress,
        };
        await _refreshTokenRepository.AddAsync(refreshToken, user.Id, ct);

        return new AuthTokenResponse(
            accessToken,
            refreshTokenResult.Token,
            ExpiresInSeconds(),
            _mapper.Map<AuthUserDto>(user),
            accountWasRecovered
        );
    }

    public int ExpiresInSeconds() => _configuration.GetValue("Jwt:ExpirationMinutes", 15) * 60;
}
