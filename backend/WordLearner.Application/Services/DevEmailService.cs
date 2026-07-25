using Microsoft.Extensions.Logging;
using WordLearner.Application.Common.Localization;
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
        string? language,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "EMAIL_VERIFICATION", language, otpCode, IOtpService.OtpExpiryMinutes);

    public Task SendLoginOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "LOGIN_OTP", language, otpCode, IOtpService.OtpExpiryMinutes);

    public Task SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "PASSWORD_RESET", language, otpCode, IOtpService.OtpExpiryMinutes);

    public Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) =>
        LogEmailAsync(
            toEmail,
            "ACCOUNT_DELETION",
            language,
            otpCode,
            IOtpService.DeleteAccountOtpExpiryMinutes
        );

    public Task SendPasswordChangedNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "PASSWORD_CHANGED", language);

    public Task SendAccountRecoveredNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    ) => LogEmailAsync(toEmail, "ACCOUNT_RECOVERED", language);

    // HTML gövde konsolda okunaksız olduğu için loglanmaz — geliştiricinin ihtiyacı olan tek
    // dinamik veri OTP kodu, o da ayrı bir alan olarak basılır.
    private Task LogEmailAsync(
        string toEmail,
        string templateCode,
        string? language,
        params object[] args
    )
    {
        var content = EmailTemplates.Resolve(templateCode, language, args);
        _logger.LogInformation(
            "[DEV EMAIL] To: {ToEmail} | Subject: {Subject} | Args: {Args}",
            toEmail,
            content.Subject,
            string.Join(", ", args)
        );
        return Task.CompletedTask;
    }
}
