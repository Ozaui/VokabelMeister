using MediatR;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.QrLogin;

// Token üretimi burada YAPILMAZ — GetQrLoginStatusCommand (web'in polling'i) Confirmed'i
// ilk okuduğunda üretir.
public record ConfirmQrLoginCommand(string QrToken) : IRequest<Unit>
{
    public int UserId { get; init; }
}

public class ConfirmQrLoginCommandHandler : IRequestHandler<ConfirmQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly ISecurityLogger _securityLogger;

    public ConfirmQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService,
        ISecurityLogger securityLogger
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(ConfirmQrLoginCommand request, CancellationToken ct)
    {
        var session = await QrLoginSessionOwnershipHelper.LoadScannedOwnedSessionAsync(
            _qrLoginSessionRepository,
            _passwordService,
            request.QrToken,
            request.UserId,
            ct
        );

        session.Status = QrLoginStatus.Confirmed;
        session.ConfirmedAt = DateTime.UtcNow;
        await _qrLoginSessionRepository.UpdateAsync(session, request.UserId, ct);

        // session.RequesterIp taranan cihazın IP'si — onaylayan zaten aynı kullanıcı
        // (sahiplik kontrolü yukarıda yapıldı), bu yüzden request.ClientIp değil bu alan kullanılır.
        await _securityLogger.LogAsync(
            LogEventType.QrLoginConfirmed,
            request.UserId,
            ipAddress: session.RequesterIp,
            ct: ct
        );

        return Unit.Value;
    }
}
