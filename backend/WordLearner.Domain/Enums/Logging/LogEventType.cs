namespace WordLearner.Domain.Enums.Logging;

public enum LogEventType
{
    LoginFailed,
    OtpFailed,
    RateLimitHit,
    UnauthorizedAccess,

    // Token Family Pattern — zaten kullanılmış bir refresh token tekrar kullanıldı.
    TokenReplay,

    PasswordReset,
    AccountDeletion,
    AdminAction,
    QrLoginConfirmed,
    QrLoginDenied,
}
