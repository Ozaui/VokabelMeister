// ─────────────────────────────────────────────────────────────────────────────
// CategoryHasChildrenException.cs
//
// AMAÇ: Alt kategorisi olan bir Category silinmeye çalışıldığında fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 409 Conflict'e çevirir — API_ENDPOINTS.md
//        §6'daki "alt kategori/aktif kelime varsa silme 409" kuralının BİRİNCİ yarısı.
//        DuplicateWordException ile birebir aynı desen (Code + sabit İngilizce log mesajı).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class CategoryHasChildrenException : AppException
{
    public CategoryHasChildrenException()
        : base("CATEGORY_HAS_CHILDREN", "Category deletion attempt: category still has child categories.")
    { }
}
