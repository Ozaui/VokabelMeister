using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class CategoryHasActiveWordsException : AppException
{
    public CategoryHasActiveWordsException(int categoryId)
        : base("CATEGORY_HAS_ACTIVE_WORDS", HttpStatusCode.Conflict, $"Category has active words: Id={categoryId}")
    {
    }
}
