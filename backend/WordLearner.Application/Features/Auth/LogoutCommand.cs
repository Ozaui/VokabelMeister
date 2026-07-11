// ─────────────────────────────────────────────────────────────────────────────
// LogoutCommand.cs
//
// AMAÇ: POST /auth/logout — verilen refresh token'ı kalıcı olarak iptal eder.
// NEDEN: Sahiplik kontrolü — başkasının refresh token'ı bu userId ile iptal edilemez.
// NEDEN ayrı tip (RefreshCommand değil): bkz. RefreshCommand.cs'teki not — aynı
//       eski RefreshRequest DTO'sunu paylaşıyorlardı, MediatR'da dönüş tipi
//       farklı olduğu için (bu dönüşsüz, Refresh ise AuthTokenResponse) artık
//       ayrı Command'lar.
// BAĞIMLILIKLAR: IRefreshTokenRepository, IPasswordService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Features.Auth;

// NEDEN UserId init-property: JWT'den (CurrentUserId) gelir, gövdede yer almaz —
//       controller model binding'den SONRA `with` ile ekler.
public record LogoutCommand(string RefreshToken) : IRequest<Unit>
{
    public int UserId { get; init; }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService
    )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken ct)
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (token is null || token.UserId != request.UserId)
            throw new InvalidRefreshTokenException();

        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token, request.UserId, ct);

        return Unit.Value;
    }
}
