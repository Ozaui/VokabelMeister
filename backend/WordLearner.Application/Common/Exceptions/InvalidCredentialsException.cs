using System.Net;

namespace WordLearner.Application.Common.Exceptions;

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS", HttpStatusCode.Unauthorized, "Email or password is incorrect.")
    {
    }
}
