namespace WordLearner.Application.Interfaces.Services;

// language, isteğin Accept-Language'ından gelir (CLAUDE.md §1 "istemciye giden mesaj" istisnası) —
// null ise şablon varsayılan dile (tr) düşer.
public interface IEmailService
{
    Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    );

    Task SendLoginOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    );

    Task SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    );

    Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    );

    Task SendPasswordChangedNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    );

    Task SendAccountRecoveredNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    );
}
