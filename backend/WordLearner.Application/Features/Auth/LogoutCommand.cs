using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Features.Auth;

// UserId JWT'den gelir, gövdede yer almaz — controller model binding'den sonra `with` ile ekler.
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
