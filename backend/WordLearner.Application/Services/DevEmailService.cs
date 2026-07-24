using Microsoft.Extensions.Logging;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

// Gerçek SMTP yerine e-postayı konsola/dosyaya (Serilog) yazar — geliştirici OTP kodunu
// konsoldan okuyup akışı uçtan uca deneyebilir.
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger) => _logger = logger;

    public Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Email Verification", $"Your verification code: {otpCode}");

    public Task SendLoginOtpAsync(string toEmail, string otpCode, CancellationToken ct = default) =>
        LogEmailAsync(toEmail, "Login Verification Code", $"Your login code: {otpCode}");

    public Task SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Password Reset", $"Your password reset code: {otpCode}");

    public Task SendPasswordChangedNotificationAsync(
        string toEmail,
        CancellationToken ct = default
    ) =>
        LogEmailAsync(
            toEmail,
            "Your Password Was Changed",
            "Your account password was just changed."
        );

    public Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "Account Deletion Confirmation", $"Your account deletion confirmation code: {otpCode}");

    // PII kuralı — ham e-posta yalnızca geliştirme ortamı konsol logunda görünür.
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
