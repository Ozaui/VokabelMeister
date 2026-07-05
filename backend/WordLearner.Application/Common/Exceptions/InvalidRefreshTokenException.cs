// ─────────────────────────────────────────────────────────────────────────────
// InvalidRefreshTokenException.cs
//
// AMAÇ: Refresh token bulunamadığında, süresi dolduğunda, iptal edilmişken veya
//       tekrar (replay) kullanıldığında fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 401 Unauthorized'a çevirir — istemci
//        bu durumda kullanıcıyı yeniden login akışına yönlendirmelidir.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException()
        : base("INVALID_REFRESH_TOKEN", "Refresh attempt: token invalid/expired/revoked.") { }
}
