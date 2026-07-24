namespace WordLearner.Application.Common.Exceptions;

// DB CHECK constraint'i ile ifade edilemez (bir satırın kendi alt ağacında olup olmadığı
// tüm zinciri gezmeden anlaşılamaz) — bu yüzden ICategoryRepository.WouldCreateCycleAsync ile kontrol edilir.
public class CategoryParentCycleException : AppException
{
    public CategoryParentCycleException()
        : base("CATEGORY_CANNOT_BE_OWN_PARENT", "Category update attempt: new parent would create a cycle in the hierarchy.")
    { }
}
