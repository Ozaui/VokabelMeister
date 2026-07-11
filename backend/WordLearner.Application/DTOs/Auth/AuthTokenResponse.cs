// ─────────────────────────────────────────────────────────────────────────────
// AuthTokenResponse.cs
//
// AMAÇ: Başarılı bir girişin (OTP doğrulama, Google, Apple, refresh, QR) ortak
//       yanıt şekli.
// NEDEN: Tüm giriş yöntemleri aynı ITokenService'i çağırıp aynı şekilde token
//        ürettiği için tek bir yanıt DTO'su yeterli — her akış kendi DTO'sunu
//        icat etmez (REFERENCE/API_ENDPOINTS.md §3 örneğiyle birebir eşleşir).
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Yanıttaki minimal kullanıcı özeti. Tam profil için GET /users/me kullanılır (C-fazı).
// NEDEN (ThemePreference): Her login yolu (OTP/Google/Apple/refresh/QR) bu DTO'yu paylaştığı için
//        tema tercihi tek yerden tüm istemcilere (Admin/Web/Mobil) yayılır — istemci, tokenı
//        aldığı anda kullanıcının kayıtlı temasını bilir, ayrı bir GET /users/me çağrısı gerekmez.
public record AuthUserDto(int Id, string CurrentLevel, string ThemePreference);

// AMAÇ: Başarılı girişin standart yanıtı.
public record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserDto User,
    bool AccountWasRecovered
);
