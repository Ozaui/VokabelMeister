/// <summary>
/// DeleteAccountRequest.cs
///
/// AMAÇ: Hesap silme onay isteği modeli — OTP kodu + şifre çift doğrulaması.
/// NEDEN: Hesap silme geri alınamaz bir işlemdir; tek doğrulama yeterli değildir.
///        OTP kodu (e-posta kanalı) + şifre (bilgi faktörü) birlikte zorunludur.
///
/// BEST PRACTICE NOTU:
///   Bu implementasyon: OTP + şifre doğrulama + anlık soft delete.
///   Üretim sistemlerinde değerlendirilebilecek ek katmanlar:
///   1. Grace period (30 gün): Hesap "silinecek" olarak işaretlenir, hemen silinmez.
///      Kullanıcı bu sürede geri alabilir. TASK-042 (Deployment) kapsamında ele alınabilir.
///   2. Veri dışa aktarma: Silmeden önce kullanıcıya tüm verilerini indirme imkânı.
///   3. GDPR anonimleşme: Soft delete kişisel veri (PII) için yeterli değildir.
///      Email, ad-soyad anonimleştirilmeli veya fiziksel olarak silinmeli.
///      Bu kural TASK-042'ye eklenmelidir.
///
/// BAĞIMLILIKLAR: IAuthService.ConfirmAccountDeletionAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Hesap silme onay isteği.
///
/// AMAÇ: POST /api/v1/auth/delete-account/confirm body'sini modellemek.
/// NEDEN: Çift doğrulama (OTP + şifre) geri dönülemez işlemi korur.
/// </summary>
public class DeleteAccountRequest
{
    /// <summary>
    /// Hesap silme e-postasıyla gönderilen 6 haneli OTP kodu.
    /// KISIT: 15 dakika geçerli, tek kullanım.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının mevcut şifresi — ikinci doğrulama faktörü.
    /// NEDEN: "Yanlışlıkla silme" ve çalınmış oturum senaryolarına karşı ek koruma.
    ///        Google ile giriş yapan kullanıcılarda NULL gönderilebilir (şifre yok).
    /// </summary>
    public string? Password { get; init; }
}
