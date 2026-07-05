// ─────────────────────────────────────────────────────────────────────────────
// InvalidCredentialsException.cs
//
// AMAÇ: Login sırasında e-posta bulunamadığında, şifre yanlış olduğunda veya
//       şifresi olmayan (sosyal girişli) bir hesapla yerel login denendiğinde
//       fırlatılır.
// NEDEN: Üç durum için de AYNI koddan (GECERSIZ_KIMLIK) çözülen AYNI mesaj kullanılır
//        — hangi durumun gerçekleştiğini istemciye asla söylemeyiz, aksi hâlde bir
//        saldırgan "hangi e-postalar kayıtlı" bilgisini deneme yanılmayla çıkarabilir.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base("GECERSIZ_KIMLIK", "Login denemesi: kimlik bilgileri geçersiz.") { }
}
