// ─────────────────────────────────────────────────────────────────────────────
// FileRequiredException.cs
//
// AMAÇ: `POST /media/images/upload` isteğinde hiç dosya gönderilmediğinde
//       (veya gönderilen dosya 0 bayt olduğunda) fırlatılır.
// NEDEN: `IFormFile file` parametresi NULLABLE (`IFormFile?`) yapılmazsa, ASP.NET
//        Core'un [ApiController] + nullable-reference-types kombinasyonu bu alanı
//        OTOMATİK "zorunlu" sayıp kendi ham `ProblemDetails` JSON'ını (projenin
//        `ApiErrorResponse` şeklini DEĞİL) döner — istemci HER hata için AYNI
//        zarfı bekler, bu istisna o tutarlılığı korur (kod denetiminde bulundu).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class FileRequiredException : AppException
{
    public FileRequiredException()
        : base("FILE_REQUIRED", "Media upload attempt: no file was provided in the request.")
    { }
}
