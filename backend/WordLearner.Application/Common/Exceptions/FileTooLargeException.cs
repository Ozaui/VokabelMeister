namespace WordLearner.Application.Common.Exceptions;

public class FileTooLargeException : AppException
{
    public FileTooLargeException()
        : base("FILE_TOO_LARGE", "Media upload attempt: file size exceeds the allowed maximum.")
    { }
}
