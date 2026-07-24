namespace WordLearner.Domain.Entities.System;

// Admin panelden yönetilen, tek satırlık (singleton) SMTP yapılandırma kaydı.
public class SmtpSettings : BaseEntity
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;

    // AES-256-CBC ile şifrelenmiş — Base64(IV + cipher); ham şifre asla DB'ye yazılmaz.
    public string PasswordEncrypted { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
