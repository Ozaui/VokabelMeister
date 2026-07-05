// ─────────────────────────────────────────────────────────────────────────────
// AccountNotActiveException.cs
//
// AMAÇ: Admin tarafından dondurulmuş (IsActive=false) bir hesapla login denendiğinde
//       fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 403 Forbidden'a çevirir — 401 (kimlik
//        hatalı) ile karıştırılmaz, çünkü kimlik doğru ama hesaba erişim yasaklı.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class AccountNotActiveException : AppException
{
    public AccountNotActiveException()
        : base("HESAP_DONDURULMUS", "Login denemesi: hesap dondurulmuş (IsActive=false).") { }
}
