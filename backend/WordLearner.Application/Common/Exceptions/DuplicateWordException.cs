// ─────────────────────────────────────────────────────────────────────────────
// DuplicateWordException.cs
//
// AMAÇ: Bir dilde zaten var olan bir Text ile yeni bir Word oluşturulmaya
//       çalışıldığında (ve `force=true` verilmediğinde) fırlatılır.
// NEDEN: ExceptionHandlingMiddleware bu tipi 409 Conflict'e çevirir; istemciye
//        giden mesaj Code (WORD_TEXT_ALREADY_EXISTS) üzerinden ErrorMessages'ten
//        dile göre çözülür — DuplicateEmailException ile birebir aynı desen.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class DuplicateWordException : AppException
{
    public DuplicateWordException()
        : base("WORD_TEXT_ALREADY_EXISTS", "Word creation attempt: text already exists for this language.")
    { }
}
