using System.Net;

namespace WordLearner.Application.Common.Exceptions;

public class WordDuplicateException : AppException
{
    public WordDuplicateException(string languageCode, string text)
        : base("WORD_DUPLICATE", HttpStatusCode.Conflict, $"Word already exists: LanguageCode={languageCode}, Text={text}")
    {
    }
}
