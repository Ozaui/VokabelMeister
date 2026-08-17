using System.Net;

namespace Zausel.Application.Common.Exceptions;

// SECURITY.md §1.4: yalnızca Code + HTTP durumu taşır, istemciye giden metni sabitlemez —
// ExceptionHandlingMiddleware Code'u ErrorMessages sözlüğünden Accept-Language'a göre çözer.
public abstract class AppException : Exception
{
    public string Code { get; }
    public HttpStatusCode StatusCode { get; }

    protected AppException(string code, HttpStatusCode statusCode, string message) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
