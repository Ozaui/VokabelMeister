// ─────────────────────────────────────────────────────────────────────────────
// MessageResponse.cs
//
// AMAÇ: Token dönmeyen auth endpoint'lerinin (login adım 1, resend-verification,
//       forgot-password, delete-account/request vb.) ortak yanıt şekli.
// NEDEN: Bu endpoint'lerin hiçbiri veri döndürmez, yalnızca "işlem tetiklendi"
//        bilgisini insan-okunur bir mesajla iletir — REFERENCE/API_ENDPOINTS.md
//        §3'teki "OTP gönderildi" örneğiyle birebir eşleşir.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

public record MessageResponse(string Message);
