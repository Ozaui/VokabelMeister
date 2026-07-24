namespace WordLearner.Application.Common.Exceptions;

public class InvalidSocialTokenException : AppException
{
    public InvalidSocialTokenException()
        : base("INVALID_SOCIAL_TOKEN", "Social login attempt: token could not be verified.") { }
}
