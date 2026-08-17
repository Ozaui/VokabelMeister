using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class AccountInactiveException : AppException
{
    public AccountInactiveException()
        : base("ACCOUNT_INACTIVE", HttpStatusCode.Forbidden, "Account has been deactivated by an administrator.")
    {
    }
}
