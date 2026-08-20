using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class CategoryParentCycleException : AppException
{
    public CategoryParentCycleException(int categoryId, int parentCategoryId)
        : base("CATEGORY_PARENT_CYCLE", HttpStatusCode.BadRequest,
            $"Category parent would create a cycle: Id={categoryId}, ParentCategoryId={parentCategoryId}")
    {
    }
}
