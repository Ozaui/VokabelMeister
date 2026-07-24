// ─────────────────────────────────────────────────────────────────────────────
// UnsupportedFileTypeException.cs
//
// AMAÇ: Yüklenen görselin uzantısı izin verilen listede (.jpg/.jpeg/.png/.webp)
//       değilse fırlatılır.
// NEDEN: Sunucuya rastgele dosya türü (ör. .exe, .html) yüklenmesini engellemek
//        için bir güvenlik kontrolü — LocalFileStorageService diske yazmadan
//        ÖNCE bu kontrolü yapar. 400 döner (varsayılan AppException statüsü).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class UnsupportedFileTypeException : AppException
{
    public UnsupportedFileTypeException()
        : base("UNSUPPORTED_FILE_TYPE", "Media upload attempt: file extension is not in the allowed list.")
    { }
}
