namespace WordLearner.Application.Common.Exceptions;

public class QrSessionForbiddenException : AppException
{
    public QrSessionForbiddenException()
        : base("QR_SESSION_FORBIDDEN", "User does not own this QR login session.") { }
}
