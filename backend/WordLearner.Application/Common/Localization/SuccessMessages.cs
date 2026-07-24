namespace WordLearner.Application.Common.Localization;

// Token dönmeyen auth endpoint'lerinin (MessageResponse) Code alanı — ErrorMessages'tan ayrı,
// çünkü kodlar anlamca farklı kümeler (ör. ACCOUNT_DELETED hata iken ACCOUNT_DELETION_CONFIRMED başarı).
public static class SuccessMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["OTP_SENT"] = new() { ["tr"] = "OTP gönderildi.", ["de"] = "OTP wurde gesendet." },
        ["EMAIL_VERIFIED"] = new() { ["tr"] = "E-posta doğrulandı.", ["de"] = "E-Mail wurde bestätigt." },
        ["VERIFICATION_CODE_SENT"] = new()
        {
            ["tr"] = "Doğrulama kodu gönderildi.",
            ["de"] = "Bestätigungscode wurde gesendet.",
        },
        ["PASSWORD_UPDATED"] = new()
        {
            ["tr"] = "Şifreniz güncellendi.",
            ["de"] = "Ihr Passwort wurde aktualisiert.",
        },
        ["PASSWORD_RESET_OTP_SENT"] = new()
        {
            ["tr"] = "Şifre sıfırlama kodu gönderildi.",
            ["de"] = "Der Code zum Zurücksetzen des Passworts wurde gesendet.",
        },
        ["ACCOUNT_DELETION_OTP_SENT"] = new()
        {
            ["tr"] = "Hesap silme onay kodu gönderildi.",
            ["de"] = "Der Bestätigungscode zur Kontolöschung wurde gesendet.",
        },
        ["ACCOUNT_DELETION_CONFIRMED"] = new()
        {
            ["tr"] = "Hesabınız silindi. 30 gün içinde tekrar giriş yaparak geri alabilirsiniz.",
            ["de"] = "Ihr Konto wurde gelöscht. Sie können es innerhalb von 30 Tagen durch erneutes Anmelden wiederherstellen.",
        },
        ["SMTP_TEST_EMAIL_SENT"] = new()
        {
            ["tr"] = "Test e-postası başarıyla gönderildi.",
            ["de"] = "Test-E-Mail wurde erfolgreich gesendet.",
        },
    };

    public static string Resolve(string code, string? language) =>
        LocalizedMessageResolver.Resolve(Messages, code, language, DefaultLanguage);
}
