// ─────────────────────────────────────────────────────────────────────────────
// IEmailService.cs
//
// AMAÇ: Uygulamanın gönderdiği tüm e-postalar (OTP, şifre değişti bildirimi vb.)
//       için sözleşme.
// NEDEN: AuthService somut bir SMTP/dev implementasyonuna değil bu arayüze bağımlı
//        olmalı — testlerde mock enjekte edilebilir (CODING_STANDARDS.md §7.4 —
//        dış servisler her zaman mock'lanır). Gerçek SMTP implementasyonu (A-10)
//        henüz yazılmadı; şimdilik DevEmailService konsola/loga yazar.
// BAĞIMLILIKLAR: Yok — saf sözleşme.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

public interface IEmailService
{
    // AMAÇ: Kayıt/yeniden gönderme sonrası e-posta doğrulama OTP kodunu gönderir.
    Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    );

    // AMAÇ: Login adım 1 sonrası 2FA OTP kodunu gönderir.
    Task SendLoginOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);

    // AMAÇ: Şifre sıfırlama isteği sonrası OTP kodunu gönderir.
    Task SendPasswordResetOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);

    // AMAÇ: Şifre başarıyla değiştirildiğinde bilgilendirme e-postası gönderir.
    // NEDEN: Kullanıcı kendisi değiştirmediyse (hesabı ele geçirilmişse) fark etmesi için.
    Task SendPasswordChangedNotificationAsync(string toEmail, CancellationToken ct = default);

    // AMAÇ: Hesap silme isteği sonrası onay OTP kodunu gönderir.
    Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    );
}
