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
            ["QR_SESSION_GONE"] = "QR kodun süresi dolmuş veya zaten kullanılmış, lütfen yeniden oluşturun.",
            ["QR_SESSION_FORBIDDEN"] = "Bu QR kod başka bir kullanıcı tarafından okutulmuş.",
            ["EMAIL_REQUIRED"] = "E-posta adresi zorunlu.",
            ["EMAIL_INVALID"] = "Geçerli bir e-posta adresi girin.",
            ["PASSWORD_REQUIRED"] = "Şifre zorunlu.",
            ["PASSWORD_TOO_SHORT"] = "Şifre en az 12 karakter olmalı.",
            ["PASSWORD_MISSING_UPPERCASE"] = "Şifre en az 1 büyük harf içermeli.",
            ["PASSWORD_MISSING_LOWERCASE"] = "Şifre en az 1 küçük harf içermeli.",
            ["PASSWORD_MISSING_DIGIT"] = "Şifre en az 1 rakam içermeli.",
            ["PASSWORD_MISSING_SPECIAL_CHAR"] = "Şifre en az 1 özel karakter (!@#$%^&*) içermeli.",
            ["FIRST_NAME_REQUIRED"] = "Ad zorunlu.",
            ["FIRST_NAME_TOO_LONG"] = "Ad en fazla 50 karakter olabilir.",
            ["LAST_NAME_REQUIRED"] = "Soyad zorunlu.",
            ["LAST_NAME_TOO_LONG"] = "Soyad en fazla 50 karakter olabilir.",
            ["OTP_CODE_REQUIRED"] = "Kod zorunlu.",
            ["OTP_CODE_INVALID_FORMAT"] = "Kod 6 haneli olmalı ve yalnızca rakam içermeli.",
            ["SOCIAL_TOKEN_REQUIRED"] = "Kimlik doğrulama jetonu zorunlu.",
            ["REFRESH_TOKEN_REQUIRED"] = "Refresh token zorunlu.",
            ["QR_TOKEN_REQUIRED"] = "QR token zorunlu.",
            ["RATE_LIMIT_EXCEEDED"] = "Çok fazla istek gönderdiniz, lütfen bir süre sonra tekrar deneyin.",
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
            ["QR_SESSION_GONE"] = "Der QR-Code ist abgelaufen oder wurde bereits verwendet, bitte neu erstellen.",
            ["QR_SESSION_FORBIDDEN"] = "Dieser QR-Code wurde von einem anderen Benutzer gescannt.",
            ["EMAIL_REQUIRED"] = "E-Mail-Adresse ist erforderlich.",
            ["EMAIL_INVALID"] = "Bitte geben Sie eine gültige E-Mail-Adresse ein.",
            ["PASSWORD_REQUIRED"] = "Passwort ist erforderlich.",
            ["PASSWORD_TOO_SHORT"] = "Das Passwort muss mindestens 12 Zeichen lang sein.",
            ["PASSWORD_MISSING_UPPERCASE"] = "Das Passwort muss mindestens einen Großbuchstaben enthalten.",
            ["PASSWORD_MISSING_LOWERCASE"] = "Das Passwort muss mindestens einen Kleinbuchstaben enthalten.",
            ["PASSWORD_MISSING_DIGIT"] = "Das Passwort muss mindestens eine Ziffer enthalten.",
            ["PASSWORD_MISSING_SPECIAL_CHAR"] = "Das Passwort muss mindestens ein Sonderzeichen (!@#$%^&*) enthalten.",
            ["FIRST_NAME_REQUIRED"] = "Vorname ist erforderlich.",
            ["FIRST_NAME_TOO_LONG"] = "Der Vorname darf höchstens 50 Zeichen lang sein.",
            ["LAST_NAME_REQUIRED"] = "Nachname ist erforderlich.",
            ["LAST_NAME_TOO_LONG"] = "Der Nachname darf höchstens 50 Zeichen lang sein.",
            ["OTP_CODE_REQUIRED"] = "Code ist erforderlich.",
            ["OTP_CODE_INVALID_FORMAT"] = "Der Code muss 6-stellig sein und darf nur Ziffern enthalten.",
            ["SOCIAL_TOKEN_REQUIRED"] = "Authentifizierungstoken ist erforderlich.",
            ["REFRESH_TOKEN_REQUIRED"] = "Refresh-Token ist erforderlich.",
            ["QR_TOKEN_REQUIRED"] = "QR-Token ist erforderlich.",
            ["RATE_LIMIT_EXCEEDED"] = "Zu viele Anfragen, bitte versuchen Sie es später erneut.",
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
