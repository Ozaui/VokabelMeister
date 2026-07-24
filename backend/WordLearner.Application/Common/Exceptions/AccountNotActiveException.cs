namespace WordLearner.Application.Common.Exceptions;

// 403, 401 DEĞİL — kimlik doğru, yalnızca hesaba erişim yasaklı (admin dondurmuş).
public class AccountNotActiveException : AppException
{
    public AccountNotActiveException()
        : base("ACCOUNT_SUSPENDED", "Login attempt: account suspended (IsActive=false).") { }
}
