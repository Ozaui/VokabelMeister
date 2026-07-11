// ─────────────────────────────────────────────────────────────────────────────
// DenyQrLoginCommand.cs
//
// AMAÇ: POST /auth/qr/{token}/deny — mobil, scan adımında gördüğü cihaz/IP
//       eşleşmiyorsa veya kullanıcı isteği tanımıyorsa girişi reddeder.
// NEDEN: REFERENCE/SECURITY.md §1.3 ADIM 3b — ConfirmQrLoginCommand ile birebir
//        aynı ön koşullar (Scanned + sahiplik), yalnızca hedef durum Denied.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService (HashToken).
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

public record DenyQrLoginCommand(string QrToken) : IRequest<Unit>
{
    // NEDEN UserId init-property: bkz. ScanQrLoginCommand.
    public int UserId { get; init; }
}

public class DenyQrLoginCommandHandler : IRequestHandler<DenyQrLoginCommand, Unit>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;

    public DenyQrLoginCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
    }

    public async Task<Unit> Handle(DenyQrLoginCommand request, CancellationToken ct)
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

        if (session.Status != QrLoginStatus.Scanned)
            throw new QrSessionGoneException();

        if (session.UserId != request.UserId)
            throw new QrSessionForbiddenException();

        session.Status = QrLoginStatus.Denied;
        await _qrLoginSessionRepository.UpdateAsync(session, request.UserId, ct);

        return Unit.Value;
    }
}
