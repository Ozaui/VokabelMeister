namespace WordLearner.Application.Common.Exceptions;

public class SmtpSettingsNotConfiguredException : AppException
{
    public SmtpSettingsNotConfiguredException()
        : base("SMTP_SETTINGS_NOT_CONFIGURED", "SMTP test attempt: no SMTP settings have been saved yet.")
    { }
}
