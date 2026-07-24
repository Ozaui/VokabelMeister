namespace WordLearner.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    );

    Task SendLoginOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);
    Task SendPasswordResetOtpAsync(string toEmail, string otpCode, CancellationToken ct = default);
    Task SendPasswordChangedNotificationAsync(string toEmail, CancellationToken ct = default);

    Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        CancellationToken ct = default
    );
}
