namespace WordLearner.Application.Common;

// Yeni dil eklemek yalnızca bu sözlüğe yeni bir üst-seviye anahtar (ör. "en") eklemekle olur —
// var olan Code'lara veya exception sınıflarına dokunulmaz (CLAUDE.md §1).
public static class ErrorMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        [DefaultLanguage] = new()
        {
            ["ENTITY_NOT_FOUND"] = "Kayıt bulunamadı.",
            ["INTERNAL_SERVER_ERROR"] = "Beklenmeyen bir hata oluştu.",
            ["ACCOUNT_ANONYMIZED"] = "Bu hesap kalıcı olarak silinmiş.",
            ["EMAIL_ALREADY_REGISTERED"] = "Bu e-posta adresi zaten kayıtlı.",
            ["INVALID_CREDENTIALS"] = "E-posta veya şifre hatalı.",
            ["ACCOUNT_INACTIVE"] = "Hesabınız yönetici tarafından dondurulmuş.",
            ["INVALID_OTP"] = "Kod geçersiz.",
            ["OTP_EXPIRED"] = "Kodun süresi dolmuş.",
            ["INVALID_REFRESH_TOKEN"] = "Oturum geçersiz, lütfen tekrar giriş yapın.",
            ["INVALID_SOCIAL_TOKEN"] = "Google/Apple ile giriş doğrulanamadı.",
        },
        ["de"] = new()
        {
            ["ENTITY_NOT_FOUND"] = "Eintrag nicht gefunden.",
            ["INTERNAL_SERVER_ERROR"] = "Ein unerwarteter Fehler ist aufgetreten.",
            ["ACCOUNT_ANONYMIZED"] = "Dieses Konto wurde dauerhaft anonymisiert.",
            ["EMAIL_ALREADY_REGISTERED"] = "Diese E-Mail-Adresse ist bereits registriert.",
            ["INVALID_CREDENTIALS"] = "E-Mail oder Passwort ist falsch.",
            ["ACCOUNT_INACTIVE"] = "Ihr Konto wurde von einem Administrator deaktiviert.",
            ["INVALID_OTP"] = "Der Code ist ungültig.",
            ["OTP_EXPIRED"] = "Der Code ist abgelaufen.",
            ["INVALID_REFRESH_TOKEN"] = "Sitzung ungültig, bitte melden Sie sich erneut an.",
            ["INVALID_SOCIAL_TOKEN"] = "Die Anmeldung mit Google/Apple konnte nicht verifiziert werden.",
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
