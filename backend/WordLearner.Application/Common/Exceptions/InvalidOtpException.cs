// ─────────────────────────────────────────────────────────────────────────────
// InvalidOtpException.cs
//
// AMAÇ: Girilen OTP kodu yanlış, süresi dolmuş veya amacı (purpose) uyuşmadığında
//       fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 400 Bad Request'e çevirir; "yanlış kod"
//        ile "süresi dolmuş kod" istemciye ayrı bilgi vermez (InvalidCredentialsException'daki
//        bilgi sızıntısı önlemiyle aynı gerekçe).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class InvalidOtpException : AppException
{
    public InvalidOtpException()
        : base("INVALID_OTP", "OTP verification: code invalid or expired.") { }
}
