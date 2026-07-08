// ─────────────────────────────────────────────────────────────────────────────
// QrSessionForbiddenException.cs
//
// AMAÇ: Bir kullanıcı, QR'ı TARAMAMIŞ olduğu bir oturumu onaylamaya/reddetmeye
//       çalıştığında fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 403 Forbidden'a çevirir — yalnızca
//        scan adımında UserId'si oturuma yazılan kullanıcı confirm/deny yapabilir
//        (REFERENCE/SECURITY.md §1.3, sahiplik kontrolü).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class QrSessionForbiddenException : AppException
{
    public QrSessionForbiddenException()
        : base("QR_SESSION_FORBIDDEN", "User does not own this QR login session.") { }
}
