// ─────────────────────────────────────────────────────────────────────────────
// DevEmailService.cs
//
// AMAÇ: IEmailService'in geliştirme ortamı implementasyonu — gerçek SMTP yerine
//       e-postayı konsola/dosyaya (Serilog) yazar.
// NEDEN: Gerçek SMTP servisi (SmtpEmailService, MailKit tabanlı) A-10'da yazılacak
//        (SMTP ayarları A-09'da DB'ye AES-256 şifreli kaydedilecek — henüz yok).
//        Bu implementasyon olmadan A-03'teki hiçbir OTP akışı test edilemezdi;
//        geliştirici konsoldaki OTP kodunu okuyup akışı uçtan uca deneyebilir.
// BAĞIMLILIKLAR: Microsoft.Extensions.Logging.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.Extensions.Logging;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger) => _logger = logger;

    // AMAÇ: Kayıt/yeniden gönderme sonrası e-posta doğrulama OTP kodunu loglar.
    public Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Email Verification", $"Your verification code: {otpCode}");

    // AMAÇ: Login adım 1 sonrası 2FA OTP kodunu loglar.
    public Task SendLoginOtpAsync(string toEmail, string otpCode, CancellationToken ct = default) =>
        LogEmailAsync(toEmail, "Login Verification Code", $"Your login code: {otpCode}");

    // AMAÇ: Şifre sıfırlama isteği sonrası OTP kodunu loglar.
    public Task SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Password Reset", $"Your password reset code: {otpCode}");

    // AMAÇ: Şifre değiştirildiğinde bilgilendirme e-postasını loglar.
    public Task SendPasswordChangedNotificationAsync(
        string toEmail,
        CancellationToken ct = default
    ) =>
        LogEmailAsync(
            toEmail,
            "Your Password Was Changed",
            "Your account password was just changed."
        );

    // AMAÇ: Hesap silme isteği sonrası onay OTP kodunu loglar.
    public Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Account Deletion Confirmation", $"Your account deletion confirmation code: {otpCode}");

    // AMAÇ: Tüm e-posta türleri için ortak loglama noktası.
    // NEDEN: PII kuralı (SECURITY.md §6) — ham e-posta yalnızca geliştirme ortamı
    //        konsol logunda görünür. A-10'da SmtpEmailService yazılınca DI kaydı
    //        ortama göre koşullu hâle getirilecek (dev→Dev, prod→Smtp); o güne kadar
    //        IEmailService her ortamda bu implementasyona çözülür.
    private Task LogEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation(
            "[DEV EMAIL] To: {ToEmail} | Subject: {Subject} | Body: {Body}",
            toEmail,
            subject,
            body
        );
        return Task.CompletedTask;
    }
}
