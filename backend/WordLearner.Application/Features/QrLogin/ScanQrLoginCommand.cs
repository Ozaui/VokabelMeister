using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

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
