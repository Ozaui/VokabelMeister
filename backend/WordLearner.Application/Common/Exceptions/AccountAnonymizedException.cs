namespace WordLearner.Application.Common.Exceptions;

// 403 — AccountNotActiveException'daki dondurma geçicidir, bu kalıcıdır (IsAnonymized=true).
public class AccountAnonymizedException : AppException
{
    public AccountAnonymizedException()
        : base("ACCOUNT_DELETED", "Login attempt: account permanently anonymized.") { }
}
