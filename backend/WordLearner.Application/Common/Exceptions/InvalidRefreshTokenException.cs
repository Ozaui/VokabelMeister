namespace WordLearner.Application.Common.Exceptions;

public class InvalidRefreshTokenException : AppException
{
    public InvalidRefreshTokenException()
        : base("INVALID_REFRESH_TOKEN", "Refresh attempt: token invalid/expired/revoked.") { }
}
