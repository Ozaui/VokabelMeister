// ─────────────────────────────────────────────────────────────────────────────
// LoginCompletionService.cs
//
// AMAÇ: ILoginCompletionService'in implementasyonu — AuthService'in eski private
//       CompleteLoginAsync/ExpiresInSeconds metotlarının birebir taşınmış hâli.
// NEDEN: OTP doğrulama/Google/Apple giriş Handler'larının ortak son adımı; kod
//        tekrarını önlemek için paylaşılan bir servise çıkarıldı.
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService,
//                ITokenService, IOtpService (Clear için), IConfiguration, IMapper (AuthProfile).
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: OTP doğrulama/Google/Apple girişlerinin ORTAK son adımı — grace period
    //       kurtarma, anonimleştirme kontrolü, giriş istatistikleri ve token üretimi.
    // NEDEN: Üç farklı giriş yönteminin (OTP, Google, Apple) hepsi aynı noktada
    //        birleşir; kod tekrarını önler (SECURITY.md §1 — Adım 2 mantığı).
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
            // NEDEN: 30 günlük grace period içinde soft-delete'li bir hesap otomatik
            //        kurtarılır (REFERENCE/SECURITY.md §1).
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

    // AMAÇ: appsettings.json'daki Jwt:ExpirationMinutes'i saniyeye çevirir.
    // NEDEN: AuthTokenResponse.ExpiresIn saniye cinsinden döner (REFERENCE/API_ENDPOINTS.md §3 örneği).
    public int ExpiresInSeconds() => _configuration.GetValue("Jwt:ExpirationMinutes", 15) * 60;
}
