using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

// ConfirmQrLoginCommandHandler ile DenyQrLoginCommandHandler'ın paylaştığı ortak ön koşul —
// bulduktan sonra ne yapılacağı (Confirmed mi Denied mi) handler'da kalır.
internal static class QrLoginSessionOwnershipHelper
{
    public static async Task<QrLoginSession> LoadScannedOwnedSessionAsync(
        IQrLoginSessionRepository repository,
        IPasswordService passwordService,
        string qrToken,
        int userId,
        CancellationToken ct
    )
    {
        var tokenHash = passwordService.HashToken(qrToken);
        var session =
            await repository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new EntityNotFoundException(typeof(QrLoginSession), tokenHash);

        if (session.IsExpired(DateTime.UtcNow))
        {
            await repository.UpdateAsync(session, ct: ct);
            throw new QrSessionGoneException();
        }

        if (session.Status != QrLoginStatus.Scanned)
            throw new QrSessionGoneException();

        if (session.UserId != userId)
            throw new QrSessionForbiddenException();

        return session;
    }
}
