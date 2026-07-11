// ─────────────────────────────────────────────────────────────────────────────
// ScanQrLoginCommand.cs
//
// AMAÇ: POST /auth/qr/{token}/scan — mobil, zaten giriş yapmış olduğu JWT'siyle
//       QR'ı taradığında oturumu Scanned'e taşır ve kendi UserId'sini yazar.
// NEDEN: REFERENCE/SECURITY.md §1.3 ADIM 2 — yalnızca Pending bir oturum taranabilir
//        (iki farklı cihazın aynı QR'ı taramasını / tekrar taramayı engeller).
//        Yanıt, oturumu İSTEYEN (web) tarafın IP/cihaz bilgisini döner — mobil
//        ekranda gösterilip kullanıcı tarafından gözle doğrulanır (relay/phishing önlemi).
// NASIL: 1) Hash'e göre oturumu bul, yoksa 404  2) Süresi geçmişse Expired'a çevir
//        + 410  3) Pending değilse (zaten taranmış/tüketilmiş) 410  4) Scanned'e
//        geçir, UserId/ScannedAt yaz  5) RequesterIp/RequesterDeviceInfo + PairingCode döner.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService (HashToken).
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

public record ScanQrLoginCommand(string QrToken) : IRequest<QrScanResponse>
{
    // NEDEN UserId init-property: JWT'den (CurrentUserId) gelir, route/body'de yer
    //       almaz — controller `with` ile ekler (bkz. LogoutCommand deseni).
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
