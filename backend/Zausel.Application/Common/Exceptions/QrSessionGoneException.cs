using System.Net;

namespace Zausel.Application.Common.Exceptions;

// Bulunamadı/süresi dolmuş/yanlış aşamada/tüketilmiş — hepsi istemciye AYNI genel koddan döner
// (InvalidRefreshTokenException ile aynı gerekçe: hangisi olduğunu ayırt etmek saldırgana bilgi sızdırır).
public class QrSessionGoneException : AppException
{
    public QrSessionGoneException()
        : base("QR_SESSION_GONE", HttpStatusCode.Gone, "QR login session not found, expired or already used.")
    {
    }
}
