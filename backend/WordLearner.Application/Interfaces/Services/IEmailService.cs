/// <summary>
/// IEmailService.cs
///
/// AMAÇ: E-posta gönderim operasyonlarının sözleşmesi.
/// NEDEN: AuthService e-posta göndermek zorunda ama e-posta altyapısı (SMTP vb.)
///        Application katmanında bilinmemeli — bağımlılık tersine çevrilir.
///        Geliştirme ortamında konsola yazan (DevEmailService), üretimde gerçek
///        SMTP kullanan (SmtpEmailService) farklı implementasyonlar kolayca takılabilir.
///
/// IMPLEMENTASYON PLANI:
///   - DevEmailService  (Infrastructure) → kodu Serilog ile loglar     — TASK-005
///   - SmtpEmailService (Infrastructure) → appsettings.json SMTP config — TASK-018
///
/// SMTP YAPILANDIRMASI (appsettings.json — TASK-018'de doldurulacak):
///   "Email": {
///     "SmtpHost": "smtp.gmail.com",
///     "SmtpPort": 587,
///     "EnableSsl": true,
///     "SmtpUsername": "",
///     "SmtpPassword": "",
///     "FromEmail": "noreply@vokabelmeister.com",
///     "FromName": "VokabelMeister"
///   }
///
/// BAĞIMLILIKLAR: -
/// </summary>

namespace WordLearner.Application.Interfaces.Services;

/// <summary>
/// E-posta gönderim arayüzü.
///
/// AMAÇ: AuthService ve diğer servislerin e-posta göndermesini soyutlamak.
/// NEDEN: E-posta altyapısı (SMTP) değiştiğinde servis kodu değişmez.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// AMAÇ: Kayıt sonrası e-posta doğrulama OTP kodunu gönderir.
    /// NEDEN: Doğrulanmamış e-posta ile sisteme giriş yapılamaz — sahte kayıtları engeller.
    /// NASIL: "Merhaba {firstName}, doğrulama kodunuz: {code} — 24 saat geçerlidir."
    ///        Kod ham olarak gönderilir (tek kullanım + kısa ömürlü; güvenli).
    /// </summary>
    Task SendEmailVerificationCodeAsync(
        string toEmail,
        string firstName,
        string code,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Şifre sıfırlama OTP kodunu kullanıcının e-postasına gönderir.
    /// NEDEN: 6 haneli kodu içeren e-posta olmadan şifre sıfırlanamaz.
    /// NASIL: "Şifre sıfırlama kodunuz: {code} — 5 dakika geçerlidir."
    /// </summary>
    Task SendPasswordResetCodeAsync(
        string toEmail,
        string firstName,
        string code,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Giriş sırasında 2FA OTP kodunu gönderir (local hesaplarda zorunlu).
    /// NEDEN: Şifre doğrulandıktan sonra ek doğrulama katmanı — hesap ele geçirilmesine karşı.
    /// NASIL: "Giriş doğrulama kodunuz: {code} — 5 dakika geçerlidir."
    ///        Google/Apple girişlerinde bu adım atlanır (zaten doğrulanmış).
    /// </summary>
    Task SendLoginOtpAsync(
        string toEmail,
        string firstName,
        string code,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Hesap silme onay OTP kodunu gönderir.
    /// NEDEN: Geri alınamaz işlem için e-posta kanalı üzerinden ikinci onay zorunlu.
    /// NASIL: "Hesap silme kodunuz: {code} — 15 dakika geçerlidir."
    ///        Kod + şifre birlikte doğrulanır (çift faktör).
    /// </summary>
    Task SendAccountDeletionCodeAsync(
        string toEmail,
        string firstName,
        string code,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Şifre başarıyla değiştirildiğinde bildirim e-postası gönderir.
    /// NEDEN: Kullanıcı haberi olmadan şifresi değiştirilmişse anında haberdar olur.
    /// NASIL: Bildirim içeriği — link veya kod içermez, sadece tarih ve IP adresi.
    /// </summary>
    Task SendPasswordChangedNotificationAsync(
        string toEmail,
        string firstName,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Hesap grace period içinde kurtarıldığında bildirim gönderir.
    /// NEDEN: Kullanıcı farkında olmadan hesabı kurtarıldıysa haberdar edilmeli.
    /// NASIL: "Hesabınız başarıyla kurtarıldı. Siz değilseniz destek ile iletişime geçin."
    /// </summary>
    Task SendAccountRecoveredNotificationAsync(
        string toEmail,
        string firstName,
        CancellationToken ct = default);
}
