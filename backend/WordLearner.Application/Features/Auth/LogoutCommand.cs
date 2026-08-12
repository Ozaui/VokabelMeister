using MediatR;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Features.Auth;

public record LogoutCommand(int UserId, string RefreshToken) : IRequest<Unit>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, IPasswordService passwordService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Bulunamazsa veya başka bir kullanıcıya aitse SESSİZCE başarı — logout idempotent olmalı,
        // başkasının token'ını manipüle etmeye çalışıldığını da ayrı bir hatayla belli etmemeli.
        if (token is null || token.UserId != request.UserId || token.RevokedAt is not null)
            return Unit.Value;

        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
