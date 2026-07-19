// ─────────────────────────────────────────────────────────────────────────────
// ConfirmQrLoginCommand.cs
//
// AMAÇ: POST /auth/qr/{token}/confirm — mobil, scan adımında gördüğü cihaz/IP/
//       pairingCode bilgisini kontrol edip girişi onayladığında oturumu
//       Confirmed'e taşır.
// NEDEN: REFERENCE/SECURITY.md §1.3 ADIM 3a — onay otomatik değildir; yalnızca
//        Scanned bir oturum VE yalnızca onu tarayan kullanıcı (sahiplik kontrolü)
//        confirm edebilir. Token üretimi burada YAPILMAZ — GetQrLoginStatusCommand
//        (web'in polling'i) Confirmed'i ilk okuduğunda ITokenService'i çağırır.
// NASIL: 1-4) QrLoginSessionOwnershipHelper.LoadScannedOwnedSessionAsync (hash'e göre
//        bul → yoksa 404 → süresi geçmişse Expired + 410 → Scanned değilse 410 →
//        UserId eşleşmiyorsa 403)  5) Confirmed'e geçir.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService (HashToken),
//                QrLoginSessionOwnershipHelper (DenyQrLoginCommandHandler ile paylaşılan
//                yükleme+doğrulama mantığı).
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.QrLogin;

public record ConfirmQrLoginCommand(string QrToken) : IRequest<Unit>
{
    // NEDEN UserId init-property: bkz. ScanQrLoginCommand.
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

        // NEDEN A-04'te eklendi (bkz. A-03.1 notu — bu entegrasyon bilinçli olarak
        //       loglama altyapısı hazır olana kadar ertelenmişti): session.RequesterIp
        //       tarayan cihazın IP'si — onaylayan zaten aynı kullanıcı (sahiplik kontrolü
        //       yukarıda yapıldı), bu yüzden request.ClientIp değil bu alan kullanılır.
        await _securityLogger.LogAsync(
            LogEventType.QrLoginConfirmed,
            request.UserId,
            ipAddress: session.RequesterIp,
            ct: ct
        );

        return Unit.Value;
    }
}
