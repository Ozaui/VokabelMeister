using System.Net;

namespace Zausel.Application.Common.Exceptions;

// Bulunamadı/süresi dolmuş/iptal edilmiş/tekrar kullanılmış (replay) — hepsi istemciye AYNI
// genel koddan döner; hangisi olduğunu ayırt etmek saldırgana bilgi sızdırır (SECURITY.md §1).
public class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException()
        : base("INVALID_REFRESH_TOKEN", HttpStatusCode.Unauthorized, "Refresh token is invalid, expired or already used.")
    {
    }
}
