// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSessionExpiryExtensions.cs
//
// AMAÇ: "Süresi geçmiş bir oturumu okuma anında Expired'a çevir" mantığını
//       Scan/Confirm/Deny/GetStatus Handler'larının HEPSİNİN paylaştığı küçük
//       bir yardımcıya çıkarır.
// NEDEN: DATABASE_SCHEMA/Auth.md — "ayrı temizlik görevi yok (YAGNI)", süre lazy
//        olarak okuma anında yorumlanır. Dört Handler da aynı iki satırı (kontrol +
//        mutasyon) tekrar edeceği için burada toplandı; ama sonrasında NE yapılacağı
//        (410 mi dönsün, yoksa 200 + "Expired" durumu mu) her Handler'da farklı
//        olduğu için karar handler'da kalır — bu yalnızca tespit+mutasyon yapar.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.Auth.QrLoginSession, QrLoginStatus enum.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

internal static class QrLoginSessionExpiryExtensions
{
    // AMAÇ: Süresi geçmiş, henüz terminal olmayan (Pending/Scanned) bir oturumu
    //       Expired'a çevirir ve süresi gerçekten geçmiş mi olduğunu döner.
    // NEDEN: Confirmed/Consumed/Denied gibi zaten terminal durumlar ExpiresAt
    //        geçse bile "Expired" olarak yeniden yazılmaz — geçmişte ne olduğu (audit) korunur.
    public static bool IsExpired(this QrLoginSession session, DateTime utcNow)
    {
        if (session.ExpiresAt >= utcNow)
            return false;

        if (session.Status is QrLoginStatus.Pending or QrLoginStatus.Scanned)
            session.Status = QrLoginStatus.Expired;

        return true;
    }
}
