namespace WordLearner.Application.Common.Exceptions;

// E-posta yok/şifre yanlış/sosyal girişli hesapla yerel login — üçü de AYNI mesajı döner,
// aksi hâlde bir saldırgan hangi e-postaların kayıtlı olduğunu deneme yanılmayla çıkarabilir.
public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS", "Login attempt: invalid credentials.") { }
}
