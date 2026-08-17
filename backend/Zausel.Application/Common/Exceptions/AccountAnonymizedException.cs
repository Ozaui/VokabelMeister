using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class AccountAnonymizedException : AppException
{
    public AccountAnonymizedException()
        : base("ACCOUNT_ANONYMIZED", HttpStatusCode.Forbidden, "Account has been permanently anonymized.")
    {
    }
}
