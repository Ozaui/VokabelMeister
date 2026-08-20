using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class FileRequiredException : AppException
{
    public FileRequiredException() : base("FILE_REQUIRED", HttpStatusCode.BadRequest, "File is required")
    {
    }
}
