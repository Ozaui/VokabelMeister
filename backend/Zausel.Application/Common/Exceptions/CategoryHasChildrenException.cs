using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class CategoryHasChildrenException : AppException
{
    public CategoryHasChildrenException(int categoryId)
        : base("CATEGORY_HAS_CHILDREN", HttpStatusCode.Conflict, $"Category has children: Id={categoryId}")
    {
    }
}
