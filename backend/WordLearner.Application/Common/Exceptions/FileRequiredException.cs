namespace WordLearner.Application.Common.Exceptions;

// IFormFile nullable yapılmazsa ASP.NET Core bu alanı otomatik "zorunlu" sayıp kendi ham
// ProblemDetails JSON'ını döner (ApiErrorResponse şeklini değil) — bu istisna tutarlılığı korur.
public class FileRequiredException : AppException
{
    public FileRequiredException()
        : base("FILE_REQUIRED", "Media upload attempt: no file was provided in the request.")
    { }
}
