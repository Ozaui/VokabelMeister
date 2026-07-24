using MailKit.Net.Smtp;
using MailKit.Security;
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
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "VokabelMeister — SMTP Test";
        message.Body = new TextPart("plain")
        {
            Text = "Bu bir test e-postasıdır. SMTP ayarlarınız doğru çalışıyor.",
        };

        try
        {
            using var client = new SmtpClient();
            var secureOption = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(settings.Host, settings.Port, secureOption, ct);
            await client.AuthenticateAsync(settings.Username, decryptedPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // MailKit farklı aşamalarda (DNS/bağlantı, kimlik doğrulama, gönderim) farklı exception
            // tipleri fırlatır — hepsi istemci için aynı anlama gelir, tek hata sözleşmesine sarılır.
            throw new SmtpTestFailedException(ex.Message);
        }
    }
}
