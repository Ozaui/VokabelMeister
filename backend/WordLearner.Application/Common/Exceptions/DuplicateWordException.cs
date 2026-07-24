namespace WordLearner.Application.Common.Exceptions;

public class DuplicateWordException : AppException
{
    public DuplicateWordException()
        : base("WORD_TEXT_ALREADY_EXISTS", "Word creation attempt: text already exists for this language.")
    { }
}
