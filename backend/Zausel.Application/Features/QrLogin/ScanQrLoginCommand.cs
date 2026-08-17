using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Application.Features.QrLogin;

public record ScanQrLoginCommand(string QrToken, int UserId) : IRequest<QrLoginScanResponse>;

public class ScanQrLoginCommandHandler : IRequestHandler<ScanQrLoginCommand, QrLoginScanResponse>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public ScanQrLoginCommandHandler(IQrLoginSessionRepository qrLoginSessionRepository, IPasswordService passwordService)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<QrLoginScanResponse> Handle(ScanQrLoginCommand request, CancellationToken cancellationToken)
    {
        var session = await _qrLoginSessionRepository.GetByTokenHashAsync(_passwordService.HashToken(request.QrToken), cancellationToken)
            ?? throw new QrSessionGoneException();

        if (session.ExpiresAt < DateTime.UtcNow || session.Status != QrLoginStatus.Pending)
            throw new QrSessionGoneException();

        session.Status = QrLoginStatus.Scanned;
        session.UserId = request.UserId;
        session.ScannedAt = DateTime.UtcNow;
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        return new QrLoginScanResponse(session.RequesterDeviceInfo, session.RequesterIp, session.PairingCode);
    }
}
