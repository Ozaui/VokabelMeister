// ─────────────────────────────────────────────────────────────────────────────
// QrSessionGoneException.cs
//
// AMAÇ: QR oturumu artık kullanılamaz durumdayken (süresi dolmuş, zaten
//       tüketilmiş/reddedilmiş, ya da beklenen aşamada değilse) fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 410 Gone'a çevirir — istemci bu
//        durumda yeni bir QR oturumu (generate) başlatmalıdır (REFERENCE/SECURITY.md §1.3).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class QrSessionGoneException : AppException
{
    public QrSessionGoneException()
        : base("QR_SESSION_GONE", "QR login session expired, consumed, or not in expected state.") { }
}
