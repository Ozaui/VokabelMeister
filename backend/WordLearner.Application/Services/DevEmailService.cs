using Microsoft.Extensions.Logging;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

// Dev ortamında SMTP kurulmadan Auth akışlarının uçtan uca test edilebilmesi için — e-posta hiç
// gönderilmez, OTP kodu [DEV EMAIL] etiketiyle loglanır. Prod'da DI, A-20'nin SmtpEmailService'ine döner.
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger) => _logger = logger;

    public Task SendEmailVerificationAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default) =>
        Log("EmailVerification", email, otpCode);

    public Task SendLoginOtpAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default) =>
        Log("LoginOtp", email, otpCode);

    public Task SendPasswordResetAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default) =>
        Log("PasswordReset", email, otpCode);

    public Task SendPasswordChangedNotificationAsync(string email, string firstName, string? language, CancellationToken cancellationToken = default) =>
        Log("PasswordChanged", email, otpCode: null);

    public Task SendAccountDeletionConfirmationAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default) =>
        Log("AccountDeletionConfirmation", email, otpCode);

    public Task SendAccountRecoveredNotificationAsync(string email, string firstName, string? language, CancellationToken cancellationToken = default) =>
        Log("AccountRecovered", email, otpCode: null);

    private Task Log(string template, string email, string? otpCode)
    {
        _logger.LogInformation("[DEV EMAIL] {Template} -> {Email} | code={Code}", template, email, otpCode ?? "-");
        return Task.CompletedTask;
    }
}
