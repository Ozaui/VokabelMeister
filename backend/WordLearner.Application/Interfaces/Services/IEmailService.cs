namespace WordLearner.Application.Interfaces.Services;

// A-20'nin 6 e-posta şablonuyla (doğrulama, login OTP, şifre sıfırlama, şifre değişti, hesap silme
// onayı, hesap kurtarıldı) birebir eşleşir — A-03 yalnızca sözleşmeyi ve dev ortamı için DevEmailService'i
// yazar, gerçek SMTP gönderimi (SmtpEmailService) A-20'de gelir. Her metot zorunlu `string? language`
// alır (SECURITY.md §1.4 ile aynı dil çözme prensibi, e-posta gövdesi de Accept-Language'a göre tr/de).
public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default);
    Task SendLoginOtpAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default);
    Task SendPasswordChangedNotificationAsync(string email, string firstName, string? language, CancellationToken cancellationToken = default);
    Task SendAccountDeletionConfirmationAsync(string email, string firstName, string otpCode, string? language, CancellationToken cancellationToken = default);
    Task SendAccountRecoveredNotificationAsync(string email, string firstName, string? language, CancellationToken cancellationToken = default);
}
