// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSessionOwnershipHelper.cs
//
// AMAÇ: "Taranmış (Scanned) VE tarayan kullanıcıya ait bir oturumu yükle" mantığını
//       ConfirmQrLoginCommandHandler ile DenyQrLoginCommandHandler'ın paylaştığı
//       küçük bir yardımcıya çıkarır.
// NEDEN: İki Handler da hash'e göre arama + süre kontrolü + Scanned kontrolü +
//        sahiplik kontrolünü birebir aynı sırada tekrar ediyordu (kod denetiminde
//        bulunan DRY ihlali) — yalnızca bulduktan SONRA ne yapılacağı (Confirmed'e
//        mi Denied'e mi geçileceği) farklı, o karar handler'da kalır. Bu yalnızca
//        yükleme+doğrulama yapar, QrLoginSessionExpiryExtensions'ın (yalnızca
//        süre kontrolü yapan) bir üst seviyesidir.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService, QrLoginSessionExpiryExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

internal static class QrLoginSessionOwnershipHelper
{
    // AMAÇ: QR token'ına karşılık gelen, süresi dolmamış, Scanned durumundaki VE
    //       verilen kullanıcıya ait oturumu döner; aksi hâlde ilgili exception'ı fırlatır.
    // NEDEN: Confirm/Deny'in "hedef duruma geçmeden önceki" ortak ön koşulu — repository
    //        ve passwordService parametre olarak alınır (DI değişikliği gerekmez),
    //        çünkü her iki Handler zaten bu iki bağımlılığı kendi constructor'ında taşıyor.
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
