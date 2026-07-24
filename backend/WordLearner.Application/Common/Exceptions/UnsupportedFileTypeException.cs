namespace WordLearner.Application.Common.Exceptions;

public class UnsupportedFileTypeException : AppException
{
    public UnsupportedFileTypeException()
        : base("UNSUPPORTED_FILE_TYPE", "Media upload attempt: file extension is not in the allowed list.")
    { }
}
