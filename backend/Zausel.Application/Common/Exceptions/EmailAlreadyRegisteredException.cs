using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class EmailAlreadyRegisteredException : AppException
{
    public EmailAlreadyRegisteredException()
        : base("EMAIL_ALREADY_REGISTERED", HttpStatusCode.Conflict, "Email is already registered.")
    {
    }
}
