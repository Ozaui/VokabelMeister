namespace WordLearner.Application.Common.Localization;

// SecurityLog.Detail'deki sabit kodların (CLAUDE.md §1 "İkinci istisna") her dildeki karşılığı —
// yazılırken değil, admin GET /admin/logs/security ile OKURKEN kendi Accept-Language'ına göre çözülür.
// ActivityLog.OldValue/NewValue burada YOK — onlar sabit kod değil, yapısal JSON diff.
public static class LogMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["LoginOtp"] = new() { ["tr"] = "Giriş OTP doğrulaması", ["de"] = "Anmelde-OTP-Verifizierung" },
        ["EmailVerification"] = new() { ["tr"] = "E-posta doğrulaması", ["de"] = "E-Mail-Verifizierung" },
        ["PasswordReset"] = new() { ["tr"] = "Şifre sıfırlama", ["de"] = "Passwort zurücksetzen" },
        ["AccountDeletion"] = new() { ["tr"] = "Hesap silme", ["de"] = "Kontolöschung" },
        ["ACCOUNT_DELETION_PASSWORD_MISMATCH"] = new()
        {
            ["tr"] = "Hesap silme onayında şifre uyuşmadı",
            ["de"] = "Passwort stimmte bei der Kontolöschungsbestätigung nicht überein",
        },
        ["TOKEN_REPLAY_FAMILY_REVOKED"] = new()
        {
            ["tr"] = "Tekrar kullanılan refresh token tespit edildi, token ailesi iptal edildi",
            ["de"] = "Wiederverwendetes Refresh-Token erkannt, Token-Familie widerrufen",
        },
        ["USER_ROLE_CHANGED"] = new() { ["tr"] = "Kullanıcı rolü değiştirildi", ["de"] = "Benutzerrolle geändert" },
        ["USER_ACCOUNT_FROZEN"] = new() { ["tr"] = "Hesap donduruldu", ["de"] = "Konto gesperrt" },
        ["USER_ACCOUNT_REACTIVATED"] = new() { ["tr"] = "Hesap yeniden aktifleştirildi", ["de"] = "Konto reaktiviert" },
        ["SMTP_SETTINGS_CHANGED"] = new() { ["tr"] = "SMTP ayarları değiştirildi", ["de"] = "SMTP-Einstellungen geändert" },
    };

    // Sözlükte olmayan bir kod (ör. RateLimitHit'in Detail'i, "/api/v1/auth/login" gibi dinamik bir
    // istek yolu) aynen geri döner — LocalizedMessageResolver bilinmeyeni kendisi olarak döner.
    public static string? Resolve(string? code, string? language) =>
        code is null ? null : LocalizedMessageResolver.Resolve(Messages, code, language, DefaultLanguage);
}
