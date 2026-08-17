using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class InvalidOtpException : AppException
{
    public InvalidOtpException()
        : base("INVALID_OTP", HttpStatusCode.BadRequest, "OTP code is invalid.")
    {
    }
}
