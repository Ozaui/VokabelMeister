using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class FileTooLargeException : AppException
{
    public FileTooLargeException() : base("FILE_TOO_LARGE", HttpStatusCode.BadRequest, "File exceeds maximum size")
    {
    }
}
