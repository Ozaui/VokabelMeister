// ─────────────────────────────────────────────────────────────────────────────
// FileTooLargeException.cs
//
// AMAÇ: Yüklenen görsel LocalFileStorageService.MaxFileSizeBytes sınırını
//       aşıyorsa fırlatılır.
// NEDEN: Disk/bant genişliği tükenmesini önlemek için bir üst sınır — kontrol
//        dosya diske yazılmadan ÖNCE, `IFormFile.Length` üzerinden yapılır.
//        400 döner (varsayılan AppException statüsü).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class FileTooLargeException : AppException
{
    public FileTooLargeException()
        : base("FILE_TOO_LARGE", "Media upload attempt: file size exceeds the allowed maximum.")
    { }
}
