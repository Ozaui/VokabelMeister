// ─────────────────────────────────────────────────────────────────────────────
// DuplicateEmailException.cs
//
// AMAÇ: Kayıt sırasında zaten kullanılan (veya daha önce anonimleştirilmiş) bir
//       e-posta adresi girildiğinde fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 409 Conflict'e çevirir; istemciye
//        giden mesaj Code (EMAIL_ALREADY_REGISTERED) üzerinden ErrorMessages'ten dile göre çözülür.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class DuplicateEmailException : AppException
{
    public DuplicateEmailException()
        : base("EMAIL_ALREADY_REGISTERED", "Registration attempt: email address already in use.") { }
}
