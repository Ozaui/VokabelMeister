// ─────────────────────────────────────────────────────────────────────────────
// AccountAnonymizedException.cs
//
// AMAÇ: 30 günlük grace period sonunda kalıcı olarak anonimleştirilmiş (IsAnonymized=true)
//       bir hesapla login denendiğinde fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 403 Forbidden'a çevirir — bu hesap artık
//        geri getirilemez (AccountNotActiveException'daki dondurma geçicidir, bu kalıcıdır).
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class AccountAnonymizedException : AppException
{
    public AccountAnonymizedException()
        : base("ACCOUNT_DELETED", "Login attempt: account permanently anonymized.") { }
}
