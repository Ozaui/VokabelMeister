namespace WordLearner.Application.Common.Exceptions;

// OTP e-postaları için — kod eline geçmeyen kullanıcıya "gönderildi" demek, onu asla gelmeyecek
// bir e-postayı beklemeye iter. Bilgilendirme e-postalarında fırlatılmaz (bkz. SmtpEmailService).
public class EmailSendFailedException : AppException
{
    public EmailSendFailedException(string developerMessage)
        : base("EMAIL_SEND_FAILED", $"Email delivery failed: {developerMessage}") { }
}
