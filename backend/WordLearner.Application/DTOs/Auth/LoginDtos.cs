// ─────────────────────────────────────────────────────────────────────────────
// LoginDtos.cs
//
// AMAÇ: 2 adımlı OTP login akışının girdi şekilleri (POST /auth/login,
//       POST /auth/login/verify-otp).
// NEDEN: Adım 1 yalnızca e-posta/şifre alır (token dönmez); adım 2 yalnızca
//        e-posta/OTP kodu alır — REFERENCE/SECURITY.md §1'deki akışla birebir eşleşir.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Login adım 1 — şifre doğrulanır, başarılıysa OTP gönderilir.
public record LoginRequest(string Email, string Password);

// AMAÇ: Login adım 2 (ve QR/sosyal giriş sonrası paylaşılan OTP şekli) — e-postaya
//       gelen 6 haneli kodu doğrular, başarılıysa token üretilir.
public record VerifyOtpRequest(string Email, string OtpCode);
