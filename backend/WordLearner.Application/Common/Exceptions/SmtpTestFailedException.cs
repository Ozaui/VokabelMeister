// ─────────────────────────────────────────────────────────────────────────────
// SmtpTestFailedException.cs
//
// AMAÇ: MailKitSmtpTestService'in SMTP sunucusuna bağlanma/kimlik doğrulama/gönderim
//       denemesi başarısız olduğunda fırlatılır.
// NEDEN 502 Bad Gateway: hata istemcinin isteğinde DEĞİL, uygulamanın bağlandığı
//       yukarı akış (upstream) bir servistedir (yanlış host/port/kimlik bilgisi ya
//       da SMTP sunucusunun kendisi erişilemez) — 400 (istemci hatası) yanıltıcı olurdu.
// NEDEN `detail` yalnızca .Message'a (log) gider, Code'a DEĞİL: gerçek MailKit hata
//       metni (ör. sunucu adresi, port) istemciye SIZDIRILMAZ — ErrorMessages'teki
//       sabit "SMTP_TEST_FAILED" mesajı dile göre döner, ayrıntı yalnızca ApplicationLog'da kalır.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class SmtpTestFailedException : AppException
{
    public SmtpTestFailedException(string detail)
        : base("SMTP_TEST_FAILED", $"SMTP test email failed: {detail}")
    { }
}
