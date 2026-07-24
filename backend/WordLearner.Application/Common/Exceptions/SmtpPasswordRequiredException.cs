// ─────────────────────────────────────────────────────────────────────────────
// SmtpPasswordRequiredException.cs
//
// AMAÇ: UpdateSmtpSettingsCommand'a, hiç SMTP ayarı KAYDEDİLMEMİŞKEN (ilk kayıt)
//       maske literal'i ("***") Password olarak gönderildiğinde fırlatılır.
// NEDEN: Maske literal'i yalnızca VAR OLAN bir şifreyi KORUMAK için bir sinyaldir
//        (UpdateSmtpSettingsCommand.cs "NEDEN MaskedPassword") — korunacak bir
//        "eski" şifre yokken bu literal'i şifrelemek (kod denetiminde bulunan bir
//        açık tasarım sorusu), DB'ye gerçek SMTP şifresi yerine "***" stringinin
//        AES ile şifrelenmiş hâlinin yazılmasına yol açardı — sessiz bir
//        yanlış-yapılandırma. FluentValidation bu kontrolü YAPAMAZ (DB'ye
//        erişimi yok, "ayar var mı" bilgisini bilemez), bu yüzden Handler
//        seviyesinde bir iş kuralı istisnasıdır. Aynı "SMTP_PASSWORD_REQUIRED"
//        kodunu (UpdateSmtpSettingsCommandValidator ile PAYLAŞIR) kullanır —
//        istemci için ikisi de anlamca AYNI şey: "geçerli bir şifre girmelisiniz".
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class SmtpPasswordRequiredException : AppException
{
    public SmtpPasswordRequiredException()
        : base("SMTP_PASSWORD_REQUIRED", "SMTP settings update attempt: masked password literal cannot be used when no settings exist yet.")
    { }
}
