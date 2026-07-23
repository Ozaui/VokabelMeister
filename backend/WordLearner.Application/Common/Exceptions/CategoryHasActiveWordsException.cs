// ─────────────────────────────────────────────────────────────────────────────
// CategoryHasActiveWordsException.cs
//
// AMAÇ: Kendisine bağlı en az bir aktif (soft-delete edilmemiş) WordConcept'i
//       olan bir Category silinmeye çalışıldığında fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 409 Conflict'e çevirir — API_ENDPOINTS.md
//        §6'daki "alt kategori/aktif kelime varsa silme 409" kuralının İKİNCİ yarısı.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class CategoryHasActiveWordsException : AppException
{
    public CategoryHasActiveWordsException()
        : base("CATEGORY_HAS_ACTIVE_WORDS", "Category deletion attempt: category still has active words linked to it.")
    { }
}
