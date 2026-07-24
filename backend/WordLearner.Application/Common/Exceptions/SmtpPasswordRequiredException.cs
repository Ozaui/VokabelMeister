namespace WordLearner.Application.Common.Exceptions;

// Maske literali ("***") yalnızca VAR OLAN bir şifreyi korumak için bir sinyaldir; ilk kayıtta
// korunacak eski şifre yokken bu literali şifrelemek DB'ye gerçek şifre yerine "***" yazardı.
// FluentValidation bu kontrolü yapamaz (DB'ye erişimi yok) — Handler seviyesinde iş kuralı.
public class SmtpPasswordRequiredException : AppException
{
    public SmtpPasswordRequiredException()
        : base("SMTP_PASSWORD_REQUIRED", "SMTP settings update attempt: masked password literal cannot be used when no settings exist yet.")
    { }
}
