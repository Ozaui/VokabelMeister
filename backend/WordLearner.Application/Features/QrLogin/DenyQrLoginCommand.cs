using MediatR;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.QrLogin;

public record DenyQrLoginCommand(string QrToken) : IRequest<Unit>
{
    public int UserId { get; init; }
}

public class DenyQrLoginCommandHandler : IRequestHandler<DenyQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly ISecurityLogger _securityLogger;

    public DenyQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService,
        ISecurityLogger securityLogger
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(DenyQrLoginCommand request, CancellationToken ct)
    {
        var session = await QrLoginSessionOwnershipHelper.LoadScannedOwnedSessionAsync(
            _qrLoginSessionRepository,
            _passwordService,
            request.QrToken,
            request.UserId,
            ct
        );

        session.Status = QrLoginStatus.Denied;
        await _qrLoginSessionRepository.UpdateAsync(session, request.UserId, ct);

        await _securityLogger.LogAsync(
            LogEventType.QrLoginDenied,
            request.UserId,
            ipAddress: session.RequesterIp,
            ct: ct
        );

        return Unit.Value;
    }
}
