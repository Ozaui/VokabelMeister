// ─────────────────────────────────────────────────────────────────────────────
// RefreshRequest.cs
//
// AMAÇ: POST /auth/refresh ve POST /auth/logout girdi şekli — ikisi de yalnızca
//       ham refresh token'ı taşır.
// NEDEN: Refresh, eskiyi geçersiz kılıp yeni token çifti üretir; logout aynı
//        token'ı DB'de kalıcı olarak iptal eder — ikisi de aynı girdiyle çalışır.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

public record RefreshRequest(string RefreshToken);
