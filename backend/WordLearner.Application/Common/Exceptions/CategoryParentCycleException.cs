// ─────────────────────────────────────────────────────────────────────────────
// CategoryParentCycleException.cs
//
// AMAÇ: Bir kategori kendisine veya kendi alt ağacındaki bir kategoriye
//       ParentCategoryId olarak bağlanmaya çalışıldığında fırlatılır.
// NEDEN: Self-ref bir hiyerarşide bu kontrol DB CHECK constraint'i ile
//        İFADE EDİLEMEZ (bir satırın kendi alt ağacında olup olmadığı, o
//        satırın KENDİSİNE bakarak anlaşılamaz — tüm zinciri gezmek gerekir);
//        bu yüzden UpdateCategoryCommandHandler'da uygulama katmanında kontrol
//        edilir (ICategoryRepository.WouldCreateCycleAsync). 400 döner (varsayılan
//        AppException statüsü) — geçersiz bir girdi, çakışma değil.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class CategoryParentCycleException : AppException
{
    public CategoryParentCycleException()
        : base("CATEGORY_CANNOT_BE_OWN_PARENT", "Category update attempt: new parent would create a cycle in the hierarchy.")
    { }
}
