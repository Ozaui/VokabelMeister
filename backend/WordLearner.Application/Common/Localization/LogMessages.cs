// ─────────────────────────────────────────────────────────────────────────────
// LogMessages.cs
//
// AMAÇ: `SecurityLog.Detail` kolonundaki sabit kodların (ör. "TOKEN_REPLAY_FAMILY_REVOKED")
//       her dildeki karşılığını tutan merkezi sözlük — CLAUDE.md §1 "İkinci istisna".
// NEDEN: SecurityLog.Detail satır YAZILIRKEN değil, admin `GET /admin/logs/security`
//        (A-07) ile OKURKEN kendi Accept-Language'ına göre çözülür — ErrorMessages/
//        SuccessMessages ile BİREBİR aynı Code-sonra-çöz deseni, yalnızca çözme ANI
//        farklı (istek anı değil, admin okuma anı).
// NEDEN yalnızca Detail (ActivityLog.Action/OldValue/NewValue BURADA YOK): CLAUDE.md'nin
//        "İkinci istisna" metni Detail/OldValue/NewValue'yü birlikte anıyor, ama gerçek
//        kod (A-05/A-06/A-07) OldValue/NewValue'yü SABİT kodlar değil, alan adı+değer
//        çiftlerinden oluşan YAPISAL JSON diff'ler olarak kullanıyor (ör. CREATE_WORD'ün
//        NewValue'su `{ PartOfSpeech, DifficultyLevel, Translations }`) — bunlar "Code"
//        sözlüğüne UYMAZ, admin panele HAM JSON olarak dönerler (bkz. GetActivityLogsQuery.cs).
//        Action da (CLAUDE.md'nin AÇIKÇA belirttiği gibi) sabit/dilden bağımsız KALIR,
//        hiç çevrilmez. Bu sözlük yalnızca gerçekten "sabit bir kod" olan Detail içindir.
// BAĞIMLILIKLAR: LocalizedMessageResolver (ErrorMessages/SuccessMessages ile PAYLAŞILAN
//                çözme algoritması).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

public static class LogMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        // A-03/A-04 — Login/OTP akışlarının SecurityLog.Detail'i (VerifyLoginOtp/VerifyEmail/
        // ResetPassword/ConfirmAccountDeletion'ın OtpFailed olayı hangi akıştan geldiğini ayırt eder).
        ["LoginOtp"] = new() { ["tr"] = "Giriş OTP doğrulaması", ["de"] = "Anmelde-OTP-Verifizierung" },
        ["EmailVerification"] = new() { ["tr"] = "E-posta doğrulaması", ["de"] = "E-Mail-Verifizierung" },
        ["PasswordReset"] = new() { ["tr"] = "Şifre sıfırlama", ["de"] = "Passwort zurücksetzen" },
        ["AccountDeletion"] = new() { ["tr"] = "Hesap silme", ["de"] = "Kontolöschung" },
        ["ACCOUNT_DELETION_PASSWORD_MISMATCH"] = new()
        {
            ["tr"] = "Hesap silme onayında şifre uyuşmadı",
            ["de"] = "Passwort stimmte bei der Kontolöschungsbestätigung nicht überein",
        },

        // A-03/A-04 — Refresh Token Family Pattern.
        ["TOKEN_REPLAY_FAMILY_REVOKED"] = new()
        {
            ["tr"] = "Tekrar kullanılan refresh token tespit edildi, token ailesi iptal edildi",
            ["de"] = "Wiederverwendetes Refresh-Token erkannt, Token-Familie widerrufen",
        },

        // A-07 — Kullanıcı Yönetimi (rol/hesap durumu değişimi).
        ["USER_ROLE_CHANGED"] = new() { ["tr"] = "Kullanıcı rolü değiştirildi", ["de"] = "Benutzerrolle geändert" },
        ["USER_ACCOUNT_FROZEN"] = new() { ["tr"] = "Hesap donduruldu", ["de"] = "Konto gesperrt" },
        ["USER_ACCOUNT_REACTIVATED"] = new() { ["tr"] = "Hesap yeniden aktifleştirildi", ["de"] = "Konto reaktiviert" },
    };

    // AMAÇ: Bir Detail koduna, istenen dile (bulunamazsa Türkçe'ye) karşılık gelen metni
    //       döner. NEDEN sözlükte olmayan bir kod (ör. RateLimitHit'in Detail'i — bir istek
    //       yolu, `/api/v1/auth/login` gibi DİNAMİK bir değer, SABİT bir kod DEĞİL) AYNEN
    //       geri döner: LocalizedMessageResolver, bilinmeyen bir kodu KENDİSİ olarak döner
    //       (exception fırlatmaz) — bu, dinamik Detail değerlerinin (URL yolu gibi) OLDUĞU
    //       GİBİ (çevrilmeden) geçmesini sağlar, aksi hâlde her rate-limit satırı "bilinmeyen
    //       kod" hatası verirdi.
    public static string? Resolve(string? code, string? language) =>
        code is null ? null : LocalizedMessageResolver.Resolve(Messages, code, language, DefaultLanguage);
}
