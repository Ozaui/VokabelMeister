// ─────────────────────────────────────────────────────────────────────────────
// QrLoginDtos.cs
//
// AMAÇ: QR ile giriş akışının 4 adımına (generate/scan/status) karşılık gelen yanıt DTO'ları.
// NEDEN: Her adımın yanıt şekli farklı (generate: token+pairingCode; scan: cihaz
//        karşılaştırma bilgisi; status: durum + yalnızca Confirmed'de token) —
//        REFERENCE/API_ENDPOINTS.md §3.1 örnekleriyle birebir eşleşir. confirm/deny
//        yanıtsız (204 No Content) olduğu için burada DTO'ları yok.
// BAĞIMLILIKLAR: AuthUserDto (AuthTokenResponse.cs).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: POST /auth/qr/generate yanıtı — QR/deep-link'e gömülecek ham token.
public record QrGenerateResponse(string QrToken, string PairingCode, int ExpiresIn);

// AMAÇ: POST /auth/qr/{token}/scan yanıtı — mobil ekranda gösterilip web'dekiyle
//       karşılaştırılan, QR'ı İSTEYEN (web) tarafın bilgisi.
public record QrScanResponse(string? RequesterDeviceInfo, string? RequesterIp, string PairingCode);

// AMAÇ: GET /auth/qr/{token}/status yanıtı — Confirmed dışındaki durumlarda yalnızca
//       Status doludur; Confirmed'de (tek seferlik) token alanları da gelir.
public record QrStatusResponse(
    string Status,
    string? AccessToken,
    string? RefreshToken,
    int? ExpiresIn,
    AuthUserDto? User
);
