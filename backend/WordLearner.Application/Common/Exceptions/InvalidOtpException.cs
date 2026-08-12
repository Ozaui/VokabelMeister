using System.Net;

namespace WordLearner.Application.Common.Exceptions;

public class InvalidOtpException : AppException
{
    public InvalidOtpException()
        : base("INVALID_OTP", HttpStatusCode.BadRequest, "OTP code is invalid.")
    {
    }
}
