// ─────────────────────────────────────────────────────────────────────────────
// OtpPurpose.cs
//
// AMAÇ: Kullanıcıya gönderilen tek kullanımlık (OTP) kodunun hangi işlem için
//       üretildiğini ayırt eden enum.
// NEDEN: User tablosunda tek bir OTP alan seti (PendingOtpCodeHash/ExpiresAt)
//        e-posta doğrulama, login 2FA, şifre sıfırlama ve hesap silme gibi
//        birden fazla akışta paylaşılır; bu enum olmadan hangi kodun hangi
//        işlem için üretildiği ayırt edilemez (biri diğeri yerine kullanılabilir).
// BAĞIMLILIKLAR: Yok — saf C# enum.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Enums.Auth;

public enum OtpPurpose
{
    // AMAÇ: Kayıt sonrası e-posta adresinin doğrulanması.
    EmailVerification,

    // AMAÇ: 2 adımlı login akışının ikinci adımı (şifre doğrulandıktan sonra).
    LoginOtp,

    // AMAÇ: "Şifremi unuttum" akışında yeni şifre belirlemeden önceki doğrulama.
    PasswordReset,

    // AMAÇ: Hesap silme isteğinin (soft delete) onaylanması.
    AccountDeletion,
}
