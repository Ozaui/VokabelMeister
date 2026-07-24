using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

// Yalnızca Pending bir oturum taranabilir — iki farklı cihazın aynı QR'ı taramasını/tekrar taramayı engeller.
public record ScanQrLoginCommand(string QrToken) : IRequest<QrScanResponse>
{
    public int UserId { get; init; }
}

public class ScanQrLoginCommandHandler : IRequestHandler<ScanQrLoginCommand, QrScanResponse>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public ScanQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<QrScanResponse> Handle(ScanQrLoginCommand request, CancellationToken ct)
    {
        var tokenHash = _passwordService.HashToken(request.QrToken);
        var session =
            await _qrLoginSessionRepository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new EntityNotFoundException(typeof(QrLoginSession), tokenHash);

        if (session.IsExpired(DateTime.UtcNow))
        {
            await _qrLoginSessionRepository.UpdateAsync(session, ct: ct);
            throw new QrSessionGoneException();
        }

        if (session.Status != QrLoginStatus.Pending)
            throw new QrSessionGoneException();

        session.Status = QrLoginStatus.Scanned;
        session.UserId = request.UserId;
        session.ScannedAt = DateTime.UtcNow;
        await _qrLoginSessionRepository.UpdateAsync(session, request.UserId, ct);

        return new QrScanResponse(session.RequesterDeviceInfo, session.RequesterIp, session.PairingCode);
    }
}
