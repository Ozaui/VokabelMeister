// ─────────────────────────────────────────────────────────────────────────────
// DeleteAccountConfirmRequest.cs
//
// AMAÇ: POST /auth/delete-account/confirm girdi şekli.
// NEDEN: Hesap silme geri alınamaz bir işlem olduğu için OTP'ye ek olarak
//        şifre de istenir (REFERENCE/API_ENDPOINTS.md §3) — çift onay.
//        request adımı (/auth/delete-account/request) girdi almaz, JWT'deki
//        kullanıcı kimliğini kullanır; bu yüzden onun için ayrı bir DTO yok.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

public record DeleteAccountConfirmRequest(string OtpCode, string Password);
