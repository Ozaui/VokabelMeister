namespace WordLearner.Application.Common;

// ErrorMessages ile simetrik: Unit döndüren Command'lar hardcoded metin taşımaz — Controller
// katmanı hangi eylemin tamamlandığını bir Code ile seçer, gerçek metin Accept-Language'a göre
// bu sözlükten çözülür. Yeni dil = ErrorMessages ile aynı desen, sözlüğe yeni bir sütun eklemek.
public static class SuccessMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        [DefaultLanguage] = new()
        {
            ["LOGIN_OTP_SENT"] = "OTP gönderildi.",
            ["VERIFICATION_RESENT"] = "Doğrulama kodu tekrar gönderildi.",
            ["EMAIL_VERIFIED"] = "E-posta doğrulandı.",
            ["LOGGED_OUT"] = "Çıkış yapıldı.",
            ["PASSWORD_RESET_OTP_SENT"] = "Şifre sıfırlama kodu gönderildi.",
            ["PASSWORD_RESET"] = "Şifreniz değiştirildi.",
            ["ACCOUNT_DELETION_OTP_SENT"] = "Hesap silme onay kodu gönderildi.",
            ["ACCOUNT_DELETED"] = "Hesabınız silindi.",
            ["QR_LOGIN_CONFIRMED"] = "QR ile giriş onaylandı.",
            ["QR_LOGIN_DENIED"] = "QR ile giriş reddedildi.",
        },
        ["de"] = new()
        {
            ["LOGIN_OTP_SENT"] = "OTP gesendet.",
            ["VERIFICATION_RESENT"] = "Bestätigungscode erneut gesendet.",
            ["EMAIL_VERIFIED"] = "E-Mail wurde bestätigt.",
            ["LOGGED_OUT"] = "Abmeldung erfolgreich.",
            ["PASSWORD_RESET_OTP_SENT"] = "Code zum Zurücksetzen des Passworts gesendet.",
            ["PASSWORD_RESET"] = "Ihr Passwort wurde geändert.",
            ["ACCOUNT_DELETION_OTP_SENT"] = "Bestätigungscode für die Kontolöschung gesendet.",
            ["ACCOUNT_DELETED"] = "Ihr Konto wurde gelöscht.",
            ["QR_LOGIN_CONFIRMED"] = "QR-Anmeldung bestätigt.",
            ["QR_LOGIN_DENIED"] = "QR-Anmeldung abgelehnt.",
        },
    };

    public static string Resolve(string code, string? language)
    {
        var resolvedLanguage = language is not null && Messages.ContainsKey(language)
            ? language
            : DefaultLanguage;

        return Messages[resolvedLanguage].TryGetValue(code, out var message) ? message : code;
    }
}
