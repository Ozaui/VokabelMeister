using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Services;

// MailKitSmtpTestService (admin'in denediği ayarlarla) ve SmtpEmailService (kayıtlı ayarlarla)
// aynı gönderim adımlarını paylaşır; hatayı nasıl karşılayacakları farklı olduğu için burada
// yakalanmaz, çağırana bırakılır.
internal static class MailKitSender
{
    public static async Task SendAsync(
        SmtpSettings settings,
        string decryptedPassword,
        string toEmail,
        string subject,
        MimeEntity body,
        CancellationToken ct
    )
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = body;

        using var client = new SmtpClient();
        var secureOption = settings.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;
        await client.ConnectAsync(settings.Host, settings.Port, secureOption, ct);
        await client.AuthenticateAsync(settings.Username, decryptedPassword, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
