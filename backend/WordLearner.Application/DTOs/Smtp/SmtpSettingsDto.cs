namespace WordLearner.Application.DTOs.Smtp;

// Password: hiç ayar kaydedilmemişse boş string, kaydedilmişse sabit "***" — gerçek şifre
// istemciye asla dönmez. Admin formu değiştirmeden geri gönderirse Handler "şifre aynı kalsın" okur.
public record SmtpSettingsDto(
    string Host,
    int Port,
    bool EnableSsl,
    string Username,
    string Password,
    string FromEmail,
    string FromName
);
