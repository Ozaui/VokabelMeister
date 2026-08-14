namespace WordLearner.Domain.Enums.Logging;

public enum LogEventType
{
    LoginFailed,
    OtpFailed,
    RateLimitHit,
    UnauthorizedAccess,
    TokenReplay,
    PasswordReset,
    AccountDeletion,
    AdminAction,
    QrLoginConfirmed,
    QrLoginDenied
}
