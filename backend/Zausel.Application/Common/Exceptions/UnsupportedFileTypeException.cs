using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class UnsupportedFileTypeException : AppException
{
    public UnsupportedFileTypeException() : base("UNSUPPORTED_FILE_TYPE", HttpStatusCode.BadRequest, "Unsupported file type")
    {
    }
}
