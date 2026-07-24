namespace WordLearner.Application.Common.Exceptions;

// Code, istemciye giden dile göre değişen mesajı taşımaz — ErrorMessages sözlüğünün anahtarıdır,
// ExceptionHandlingMiddleware Accept-Language'a göre çözer. .Message yalnızca log/DB için sabit İngilizce.
// EntityNotFoundException bilinçli olarak bundan türemez — mesajı dinamik veri içerir.
public abstract class AppException : Exception
{
    public string Code { get; }

    protected AppException(string code, string developerMessage)
        : base(developerMessage)
    {
        Code = code;
    }
}
