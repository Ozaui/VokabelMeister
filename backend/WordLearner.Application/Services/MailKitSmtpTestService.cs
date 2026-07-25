using MimeKit;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Services;

public class MailKitSmtpTestService : ISmtpTestService
{
    public async Task SendTestEmailAsync(
        SmtpSettings settings,
        string decryptedPassword,
        string toEmail,
        CancellationToken ct = default
    )
    {
        var body = new TextPart("plain")
        {
            Text = "Bu bir test e-postasıdır. SMTP ayarlarınız doğru çalışıyor.",
        };

        try
        {
            await MailKitSender.SendAsync(
                settings,
                decryptedPassword,
                toEmail,
                "VokabelMeister — SMTP Test",
                body,
                ct
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // MailKit farklı aşamalarda (DNS/bağlantı, kimlik doğrulama, gönderim) farklı exception
            // tipleri fırlatır — hepsi istemci için aynı anlama gelir, tek hata sözleşmesine sarılır.
            throw new SmtpTestFailedException(ex.Message);
        }
    }
}
