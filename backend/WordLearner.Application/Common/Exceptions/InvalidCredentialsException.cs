// ─────────────────────────────────────────────────────────────────────────────
// InvalidCredentialsException.cs
//
// AMAÇ: Login sırasında e-posta bulunamadığında, şifre yanlış olduğunda veya
//       şifresi olmayan (sosyal girişli) bir hesapla yerel login denendiğinde
//       fırlatılır.
// NEDEN: Üç durum için de AYNI koddan (INVALID_CREDENTIALS) çözülen AYNI mesaj kullanılır
//        — hangi durumun gerçekleştiğini istemciye asla söylemeyiz, aksi hâlde bir
//        saldırgan "hangi e-postalar kayıtlı" bilgisini deneme yanılmayla çıkarabilir.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS", "Login attempt: invalid credentials.") { }
}
