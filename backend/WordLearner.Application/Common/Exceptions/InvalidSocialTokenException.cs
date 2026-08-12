using System.Net;

namespace WordLearner.Application.Common.Exceptions;

public class InvalidSocialTokenException : AppException
{
    public InvalidSocialTokenException()
        : base("INVALID_SOCIAL_TOKEN", HttpStatusCode.Unauthorized, "Google/Apple identity token could not be verified.")
    {
    }
}
