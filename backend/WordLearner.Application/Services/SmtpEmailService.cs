using Microsoft.Extensions.Logging;
using MimeKit;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

// SMTP bilgileri appsettings'te değil DB'de şifreli durur (A-09) — admin panelden değiştirilebilsin
// diye her gönderimde okunur, uygulama yeniden başlatılmaz.
public class SmtpEmailService : IEmailService
{
    private readonly ISmtpSettingsRepository _smtpSettingsRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        ISmtpSettingsRepository smtpSettingsRepository,
        IEncryptionService encryptionService,
        ILogger<SmtpEmailService> logger
    )
    {
        _smtpSettingsRepository = smtpSettingsRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public Task SendEmailVerificationOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) =>
        SendCriticalAsync(
            toEmail,
            "EMAIL_VERIFICATION",
            language,
            ct,
            otpCode,
            IOtpService.OtpExpiryMinutes
        );

    public Task SendLoginOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) => SendCriticalAsync(toEmail, "LOGIN_OTP", language, ct, otpCode, IOtpService.OtpExpiryMinutes);

    public Task SendPasswordResetOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) =>
        SendCriticalAsync(
            toEmail,
            "PASSWORD_RESET",
            language,
            ct,
            otpCode,
            IOtpService.OtpExpiryMinutes
        );

    public Task SendAccountDeletionOtpAsync(
        string toEmail,
        string otpCode,
        string? language,
        CancellationToken ct = default
    ) =>
        SendCriticalAsync(
            toEmail,
            "ACCOUNT_DELETION",
            language,
            ct,
            otpCode,
            IOtpService.DeleteAccountOtpExpiryMinutes
        );

    public Task SendPasswordChangedNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    ) => SendInformationalAsync(toEmail, "PASSWORD_CHANGED", language, ct);

    public Task SendAccountRecoveredNotificationAsync(
        string toEmail,
        string? language,
        CancellationToken ct = default
    ) => SendInformationalAsync(toEmail, "ACCOUNT_RECOVERED", language, ct);

    // OTP e-postası gitmezse akış tamamlanamaz — kullanıcıya "gönderildi" demek onu asla
    // gelmeyecek bir kodu beklemeye iter, bu yüzden hata istemciye yansıtılır.
    private async Task SendCriticalAsync(
        string toEmail,
        string templateCode,
        string? language,
        CancellationToken ct,
        params object[] args
    )
    {
        try
        {
            await SendAsync(toEmail, templateCode, language, args, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // SmtpSettingsNotConfiguredException dahil hepsi sarılır — "önce SMTP ayarlarını
            // kaydedin" admin'e söylenecek bir cümledir, kayıt olmaya çalışan kullanıcıya değil.
            throw new EmailSendFailedException(ex.Message);
        }
    }

    // Bilgilendirme e-postası, asıl işlem (şifre değişimi / hesap kurtarma) BİTTİKTEN sonra gider —
    // gönderim hatası o işlemi geri almaz, bu yüzden akışı kesmez, yalnızca loglanır.
    private async Task SendInformationalAsync(
        string toEmail,
        string templateCode,
        string? language,
        CancellationToken ct
    )
    {
        try
        {
            await SendAsync(toEmail, templateCode, language, [], ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // PII kuralı — ham e-posta loglanmaz, hangi şablonun düştüğü teşhis için yeterli.
            _logger.LogError(ex, "Informational email could not be sent: {TemplateCode}", templateCode);
        }
    }

    private async Task SendAsync(
        string toEmail,
        string templateCode,
        string? language,
        object[] args,
        CancellationToken ct
    )
    {
        var settings =
            await _smtpSettingsRepository.GetCurrentAsync(ct)
            ?? throw new SmtpSettingsNotConfiguredException();

        var content = EmailTemplates.Resolve(templateCode, language, args);
        var body = new TextPart("html") { Text = content.HtmlBody };

        await MailKitSender.SendAsync(
            settings,
            _encryptionService.Decrypt(settings.PasswordEncrypted),
            toEmail,
            content.Subject,
            body,
            ct
        );
    }
}
