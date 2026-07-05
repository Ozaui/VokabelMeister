// ─────────────────────────────────────────────────────────────────────────────
// PasswordResetDtos.cs
//
// AMAÇ: POST /auth/forgot-password ve POST /auth/reset-password girdi şekilleri.
// NEDEN: 2 adımlı OTP tabanlı şifre sıfırlama akışı (REFERENCE/SECURITY.md §7).
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Adım 1 — kullanıcı yoksa bile aynı yanıt döner (e-posta numaralandırma önlemi).
public record ForgotPasswordRequest(string Email);

// AMAÇ: Adım 2 — OTP + yeni şifre. Başarılıysa tüm cihazlardan çıkış yapılır.
public record ResetPasswordRequest(string Email, string OtpCode, string NewPassword);
