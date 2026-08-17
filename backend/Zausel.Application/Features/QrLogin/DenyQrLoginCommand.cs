using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Features.QrLogin;

public record DenyQrLoginCommand(string QrToken, int UserId, string? DeviceInfo, string? IpAddress) : IRequest<Unit>;

public class DenyQrLoginCommandHandler : IRequestHandler<DenyQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly ISecurityLogger _securityLogger;

    public DenyQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository, IPasswordService passwordService, ISecurityLogger securityLogger)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(DenyQrLoginCommand request, CancellationToken cancellationToken)
    {
        var session = await _qrLoginSessionRepository.GetByTokenHashAsync(_passwordService.HashToken(request.QrToken), cancellationToken)
            ?? throw new QrSessionGoneException();

        if (session.ExpiresAt < DateTime.UtcNow || session.Status != QrLoginStatus.Scanned)
            throw new QrSessionGoneException();

        if (session.UserId != request.UserId)
            throw new QrSessionForbiddenException();

        session.Status = QrLoginStatus.Denied;
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        await _securityLogger.LogAsync(LogEventType.QrLoginDenied, userId: request.UserId,
            ipAddress: request.IpAddress, userAgent: request.DeviceInfo, detail: "QR_LOGIN_DENIED",
            cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
