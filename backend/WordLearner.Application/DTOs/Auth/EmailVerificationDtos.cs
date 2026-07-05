// ─────────────────────────────────────────────────────────────────────────────
// EmailVerificationDtos.cs
//
// AMAÇ: POST /auth/verify-email ve POST /auth/resend-verification girdi şekilleri.
// NEDEN: Her ikisi de e-posta doğrulama akışının parçası, tek dosyada toplandı.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Kayıt sonrası e-postaya gelen 6 haneli kodu doğrular.
public record VerifyEmailRequest(string Email, string OtpCode);

// AMAÇ: Doğrulama kodunun süresi dolduysa/gelmediyse yenisini ister.
public record ResendVerificationRequest(string Email);
