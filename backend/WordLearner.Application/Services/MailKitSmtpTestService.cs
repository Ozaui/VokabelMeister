// ─────────────────────────────────────────────────────────────────────────────
// MailKitSmtpTestService.cs
//
// AMAÇ: ISmtpTestService'in MailKit tabanlı implementasyonu — verilen SMTP
//       ayarlarıyla gerçekten bağlanıp bir test e-postası gönderir.
// NEDEN try/catch içinde tek bir SmtpTestFailedException'a sarılır: MailKit farklı
//       aşamalarda (DNS/bağlantı, kimlik doğrulama, gönderim) FARKLI exception tipleri
//       fırlatabilir (SocketException, AuthenticationException, SmtpCommandException…)
//       — hepsi istemci için AYNI anlama gelir ("test başarısız"), projenin TEK hata
//       sözleşmesi (ApiErrorResponse) bu çeşitliliği istemciye YANSITMAMALI.
// BAĞIMLILIKLAR: MailKit.Net.Smtp, MimeKit.
// ─────────────────────────────────────────────────────────────────────────────

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
            // NEDEN EnableSsl→StartTls/None: entity yalnızca tek bir bool taşıyor
            //       (DATABASE_SCHEMA/Sistem.md) — MailKit'in Auto seçeneği yerine
            //       açık bir eşleme, hangi modun kullanıldığını kod okuyanına
            //       (ve BAĞLANTI hatası aldığında admin'e) belirsizlik bırakmaz.
            var secureOption = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(settings.Host, settings.Port, secureOption, ct);
            await client.AuthenticateAsync(settings.Username, decryptedPassword, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new SmtpTestFailedException(ex.Message);
        }
    }
}
