using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

// Ayrı bir temizlik job'ı yok (YAGNI) — süre lazy olarak okuma anında yorumlanır.
internal static class QrLoginSessionExpiryExtensions
{
    // Confirmed/Consumed/Denied gibi terminal durumlar ExpiresAt geçse bile Expired'a
    // yeniden yazılmaz — geçmişte ne olduğu (audit) korunur.
    public static bool IsExpired(this QrLoginSession session, DateTime utcNow)
    {
        if (session.ExpiresAt >= utcNow)
            return false;

        if (session.Status is QrLoginStatus.Pending or QrLoginStatus.Scanned)
            session.Status = QrLoginStatus.Expired;

        return true;
    }
}
