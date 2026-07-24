namespace WordLearner.Application.Common.Exceptions;

public class CategoryHasActiveWordsException : AppException
{
    public CategoryHasActiveWordsException()
        : base("CATEGORY_HAS_ACTIVE_WORDS", "Category deletion attempt: category still has active words linked to it.")
    { }
}
