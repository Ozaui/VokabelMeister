// ─────────────────────────────────────────────────────────────────────────────
// InvalidSocialTokenException.cs
//
// AMAÇ: Google/Apple'dan gelen kimlik token'ı doğrulanamadığında (imza geçersiz,
//       süresi dolmuş, audience uyuşmuyor) fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 401 Unauthorized'a çevirir —
//        IGoogleTokenValidator/IAppleTokenValidator null döndüğünde AuthService
//        bu exception'ı fırlatır.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class InvalidSocialTokenException : AppException
{
    public InvalidSocialTokenException()
        : base("GECERSIZ_SOSYAL_TOKEN", "Sosyal giriş denemesi: token doğrulanamadı.") { }
}
