using System.Net;

namespace WordLearner.Application.Common.Exceptions;

public class OtpExpiredException : AppException
{
    public OtpExpiredException()
        : base("OTP_EXPIRED", HttpStatusCode.BadRequest, "OTP code has expired.")
    {
    }
}
