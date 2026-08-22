using System.Net;

namespace Zausel.Application.Common.Exceptions;

public class UserCardDuplicateException : AppException
{
    public UserCardDuplicateException(string frontText)
        : base("USER_CARD_DUPLICATE", HttpStatusCode.Conflict, $"User card already exists: FrontText={frontText}")
    {
    }
}
