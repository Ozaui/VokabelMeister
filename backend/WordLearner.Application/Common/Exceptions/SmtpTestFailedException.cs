namespace WordLearner.Application.Common.Exceptions;

// detail yalnızca .Message'a (log) gider, Code'a değil — gerçek MailKit hata metni
// (sunucu adresi, port vb.) istemciye sızdırılmaz, ayrıntı yalnızca ApplicationLog'da kalır.
public class SmtpTestFailedException : AppException
{
    public SmtpTestFailedException(string detail)
        : base("SMTP_TEST_FAILED", $"SMTP test email failed: {detail}")
    { }
}
