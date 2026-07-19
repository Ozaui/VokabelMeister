// ─────────────────────────────────────────────────────────────────────────────
// RefreshCommand.cs
//
// AMAÇ: POST /auth/refresh — refresh token'ı doğrular, rotate eder, yeni
//       access+refresh token çifti üretir.
// NEDEN: Token Family Pattern (REFERENCE/SECURITY.md §1) — eski token tek kullanımlık;
//        aynı family'den ikinci kullanım replay sayılır ve TÜM family iptal edilir.
// NASIL: 1) Hash'e göre token'ı bul  2) Geçersiz/süresi dolmuş/iptalse reddet
//        3) Zaten kullanılmışsa (replay) family'yi iptal et ve reddet  4) Kullanıldı
//        işaretle  5) Aynı family'de yeni bir token üret.
// NEDEN ayrı tip (LogoutCommand değil): Eskiden RefreshRequest hem bu akış hem
//       de logout tarafından paylaşılıyordu; MediatR'da bir IRequest<T> tek bir
//       dönüş tipine bağlı olduğundan (bu AuthTokenResponse, Logout ise dönüşsüz)
//       artık iki ayrı Command olmaları gerekiyor (bkz. LogoutCommand.cs, wiki INGEST notu).
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService,
//                ITokenService, ILoginCompletionService (yalnızca ExpiresInSeconds için),
//                IMapper (AuthProfile).
// ─────────────────────────────────────────────────────────────────────────────

using AutoMapper;
using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

// NEDEN ClientIp init-property: bkz. VerifyLoginOtpCommand.
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
            // NEDEN Detail bir KOD (serbest metin DEĞİL): Admin panel de bir istemci ve kendi
            //       dil tercihine göre görüntülenmeli (CLAUDE.md'nin "istemciye giden mesaj"
            //       ayrımı burada da geçerli) — ama bu log SATIRI yazılırken (şu an, anonim bir
            //       isteğin Accept-Language'ıyla) hangi admin'in ne zaman hangi dille okuyacağı
            //       bilinmez. Bu yüzden ErrorMessages/SuccessMessages ile AYNI desen: burada
            //       sabit bir Code saklanır, tr/de çözümü admin'in KENDİ Accept-Language'ıyla
            //       `GET /admin/logs/security` (A-07) okurken yapılır — bkz. TASK/
            //       A_admin_panel_backend.md A-07 notu.
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
