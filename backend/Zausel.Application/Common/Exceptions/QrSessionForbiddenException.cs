using System.Net;

namespace Zausel.Application.Common.Exceptions;

// Confirm/Deny çağıran kullanıcı, session'ı tarayan (Scanned aşamasında UserId'yi dolduran)
// kullanıcı değilse — başkasının taradığı bir QR'ı onaylamaya/reddetmeye çalışma.
public class QrSessionForbiddenException : AppException
{
    public QrSessionForbiddenException()
        : base("QR_SESSION_FORBIDDEN", HttpStatusCode.Forbidden, "QR login session belongs to a different user.")
    {
    }
}
