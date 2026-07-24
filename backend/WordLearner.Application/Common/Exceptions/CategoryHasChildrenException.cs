namespace WordLearner.Application.Common.Exceptions;

public class CategoryHasChildrenException : AppException
{
    public CategoryHasChildrenException()
        : base("CATEGORY_HAS_CHILDREN", "Category deletion attempt: category still has child categories.")
    { }
}
