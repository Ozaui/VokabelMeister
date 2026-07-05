// ─────────────────────────────────────────────────────────────────────────────
// IAuthService.cs
//
// AMAÇ: Auth API'nin 13 endpoint'inin arkasındaki iş mantığı sözleşmesi.
// NEDEN: AuthController (bir sonraki task) ince katman kalsın diye tüm iş kuralı
//        bu servise toplanır; controller yalnızca DTO'yu alır, servisi çağırır,
//        sonucu döner.
// BAĞIMLILIKLAR: WordLearner.Application.DTOs.Auth.*.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Interfaces.Services;

public interface IAuthService
{
    // AMAÇ: Yeni kullanıcı kaydı oluşturur, e-posta doğrulama OTP'si gönderir.
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    // AMAÇ: Kayıt sonrası e-postaya gelen OTP kodunu doğrular.
    Task<MessageResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken ct = default
    );

    // AMAÇ: E-posta doğrulama kodunu tekrar gönderir.
    Task<MessageResponse> ResendVerificationAsync(
        ResendVerificationRequest request,
        CancellationToken ct = default
    );

    // AMAÇ: Login adım 1 — şifreyi doğrular, başarılıysa OTP gönderir (token DÖNMEZ).
    Task<MessageResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    // AMAÇ: Login adım 2 — OTP'yi doğrular, başarılıysa access+refresh token üretir.
    Task<AuthTokenResponse> VerifyLoginOtpAsync(
        VerifyOtpRequest request,
        string? ipAddress,
        CancellationToken ct = default
    );

    // AMAÇ: Google ID token'ı ile giriş yapar/kayıt olur (2FA gerekmez, sağlayıcı zaten doğruladı).
    Task<AuthTokenResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken ct = default
    );

    // AMAÇ: Apple identity token'ı ile giriş yapar/kayıt olur.
    Task<AuthTokenResponse> LoginWithAppleAsync(
        AppleLoginRequest request,
        string? ipAddress,
        CancellationToken ct = default
    );

    // AMAÇ: Refresh token'ı doğrular, rotate eder (Token Family Pattern), yeni token çifti üretir.
    Task<AuthTokenResponse> RefreshAsync(
        RefreshRequest request,
        string? ipAddress,
        CancellationToken ct = default
    );

    // AMAÇ: Verilen refresh token'ı kalıcı olarak iptal eder (yalnızca sahibi).
    Task LogoutAsync(int userId, RefreshRequest request, CancellationToken ct = default);

    // AMAÇ: Şifre sıfırlama OTP'si gönderir (kullanıcı yoksa bile aynı yanıt döner).
    Task<MessageResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken ct = default
    );

    // AMAÇ: OTP + yeni şifre ile şifreyi değiştirir, tüm cihazlardan çıkış yapar.
    Task<MessageResponse> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default
    );

    // AMAÇ: Hesap silme OTP'si gönderir (15dk geçerli).
    Task<MessageResponse> RequestAccountDeletionAsync(int userId, CancellationToken ct = default);

    // AMAÇ: OTP + şifre ile hesap silmeyi onaylar; soft delete + 30 gün grace zamanlar.
    Task<MessageResponse> ConfirmAccountDeletionAsync(
        int userId,
        DeleteAccountConfirmRequest request,
        CancellationToken ct = default
    );
}
