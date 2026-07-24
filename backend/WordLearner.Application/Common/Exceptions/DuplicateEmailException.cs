namespace WordLearner.Application.Common.Exceptions;

public class DuplicateEmailException : AppException
{
    public DuplicateEmailException()
        : base("EMAIL_ALREADY_REGISTERED", "Registration attempt: email address already in use.") { }
}
