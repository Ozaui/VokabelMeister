// ─────────────────────────────────────────────────────────────────────────────
// ErrorMessages.cs
//
// AMAÇ: AppException.Code değerlerinin her dildeki karşılığını tutan merkezi sözlük.
// NEDEN: Exception'lar mesajı kendi içinde sabitlemez — aynı kod, isteğin diline
//        göre farklı bir metne çevrilebilsin diye (REFERENCE/API_ENDPOINTS.md §1).
//        Şu an yalnızca tr+de var — uygulamanın gerçek hedef kitlesi DE↔TR
//        (bkz. DATABASE_SCHEMA/Icerik.md, Languages seed). İngilizce gibi henüz
//        hiçbir gerçek istemcinin istemediği bir dil spekülatif olarak eklenmez
//        (YAGNI — bkz. TASK.md "Spekülatif ortak tip yazılmaz" kuralı, aynı
//        gerekçeyle ApiResponse<T>/PagedResult<T> A-02'de geri alınmıştı).
//        Yeni bir dil eklemek yalnızca buraya bir sütun eklemekle olur, hiçbir
//        exception sınıfına dokunulmaz.
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

public static class ErrorMessages
{
    // NEDEN varsayılan "tr": İstekte Accept-Language yoksa veya bilinmeyen bir dilse,
    //        proje Türkçe öncelikli olduğu için Türkçe'ye düşülür (00_INDEX.md kuralı).
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["INVALID_CREDENTIALS"] = new()
        {
            ["tr"] = "E-posta veya şifre hatalı.",
            ["de"] = "E-Mail oder Passwort ist falsch.",
        },
        ["INVALID_OTP"] = new()
        {
            ["tr"] = "Girilen kod geçersiz veya süresi dolmuş.",
            ["de"] = "Der eingegebene Code ist ungültig oder abgelaufen.",
        },
        ["EMAIL_ALREADY_REGISTERED"] = new()
        {
            ["tr"] = "Bu e-posta adresi zaten kayıtlı.",
            ["de"] = "Diese E-Mail-Adresse ist bereits registriert.",
        },
        ["ACCOUNT_SUSPENDED"] = new()
        {
            ["tr"] = "Hesabınız dondurulmuş. Lütfen destekle iletişime geçin.",
            ["de"] = "Ihr Konto wurde gesperrt. Bitte wenden Sie sich an den Support.",
        },
        ["ACCOUNT_DELETED"] = new()
        {
            ["tr"] = "Bu hesap kalıcı olarak silinmiş.",
            ["de"] = "Dieses Konto wurde dauerhaft gelöscht.",
        },
        ["INVALID_REFRESH_TOKEN"] = new()
        {
            ["tr"] = "Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.",
            ["de"] = "Ihre Sitzung ist abgelaufen. Bitte melden Sie sich erneut an.",
        },
        ["INVALID_SOCIAL_TOKEN"] = new()
        {
            ["tr"] = "Sosyal giriş doğrulanamadı.",
            ["de"] = "Die soziale Anmeldung konnte nicht verifiziert werden.",
        },
        ["QR_SESSION_GONE"] = new()
        {
            ["tr"] = "QR kodunun süresi doldu veya zaten kullanıldı. Lütfen yeni bir kod oluşturun.",
            ["de"] = "Der QR-Code ist abgelaufen oder wurde bereits verwendet. Bitte erstellen Sie einen neuen Code.",
        },
        ["QR_SESSION_FORBIDDEN"] = new()
        {
            ["tr"] = "Bu QR oturumunu yalnızca onu tarayan cihaz onaylayabilir/reddedebilir.",
            ["de"] = "Diese QR-Sitzung kann nur von dem Gerät bestätigt/abgelehnt werden, das sie gescannt hat.",
        },

        // NEDEN bu blok: FluentValidation validator'ları (Application/Validators/Auth/)
        //       WithMessage() ile yalnızca sabit İngilizce bir LOG mesajı taşır — istemciye
        //       giden gerçek mesaj, her kuralın WithErrorCode() ile taşıdığı bu kodlar
        //       üzerinden ValidationFilter tarafından buradan çözülür (AppException ile
        //       birebir aynı ayrım: log=sabit İngilizce, API yanıtı=dile göre).
        ["EMAIL_REQUIRED"] = new()
        {
            ["tr"] = "E-posta adresi zorunludur.",
            ["de"] = "E-Mail-Adresse ist erforderlich.",
        },
        ["EMAIL_INVALID"] = new()
        {
            ["tr"] = "Geçerli bir e-posta adresi girin.",
            ["de"] = "Geben Sie eine gültige E-Mail-Adresse ein.",
        },
        ["PASSWORD_REQUIRED"] = new()
        {
            ["tr"] = "Şifre zorunludur.",
            ["de"] = "Passwort ist erforderlich.",
        },
        ["PASSWORD_TOO_SHORT"] = new()
        {
            ["tr"] = "Şifre en az 12 karakter olmalı.",
            ["de"] = "Das Passwort muss mindestens 12 Zeichen lang sein.",
        },
        ["PASSWORD_MISSING_UPPERCASE"] = new()
        {
            ["tr"] = "Şifre en az 1 büyük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Großbuchstaben enthalten.",
        },
        ["PASSWORD_MISSING_LOWERCASE"] = new()
        {
            ["tr"] = "Şifre en az 1 küçük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Kleinbuchstaben enthalten.",
        },
        ["PASSWORD_MISSING_DIGIT"] = new()
        {
            ["tr"] = "Şifre en az 1 rakam içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Ziffer enthalten.",
        },
        ["PASSWORD_MISSING_SPECIAL_CHAR"] = new()
        {
            ["tr"] = "Şifre en az 1 özel karakter içermeli (!@#$%^&*).",
            ["de"] = "Das Passwort muss mindestens 1 Sonderzeichen enthalten (!@#$%^&*).",
        },
        ["OTP_REQUIRED"] = new()
        {
            ["tr"] = "Doğrulama kodu zorunludur.",
            ["de"] = "Bestätigungscode ist erforderlich.",
        },
        ["OTP_INVALID_FORMAT"] = new()
        {
            ["tr"] = "Doğrulama kodu 6 haneli olmalı.",
            ["de"] = "Der Bestätigungscode muss 6-stellig sein.",
        },
        ["FIRST_NAME_REQUIRED"] = new() { ["tr"] = "Ad zorunludur.", ["de"] = "Vorname ist erforderlich." },
        ["LAST_NAME_REQUIRED"] = new()
        {
            ["tr"] = "Soyad zorunludur.",
            ["de"] = "Nachname ist erforderlich.",
        },
        ["TOKEN_REQUIRED"] = new()
        {
            ["tr"] = "Token zorunludur.",
            ["de"] = "Token ist erforderlich.",
        },

        // NEDEN bu kod: ExceptionHandlingMiddleware, AppException'dan türemeyen (beklenmeyen)
        //       her exception için bu kodu kullanır — gerçek exception mesajı istemciye asla
        //       sızdırılmaz, sabit ve dile göre çözülen bir mesaj döner.
        ["INTERNAL_SERVER_ERROR"] = new()
        {
            ["tr"] = "Beklenmeyen bir hata oluştu.",
            ["de"] = "Ein unerwarteter Fehler ist aufgetreten.",
        },
    };

    // AMAÇ: Bir hata koduna, istenen dile (bulunamazsa Türkçe'ye) karşılık gelen mesajı döner.
    // NEDEN: Sözlükte olmayan bir kod gelirse (programlama hatası — yeni bir AppException
    //        eklenip buraya çevirisi eklenmemişse) exception fırlatmak yerine kodun kendisi
    //        döner; API asla yalnızca çeviri eksik diye 500'e düşmemeli.
    public static string Resolve(string code, string? language)
    {
        if (!Messages.TryGetValue(code, out var translations))
            return code;

        var lang = string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language;
        return translations.TryGetValue(lang, out var message)
            ? message
            : translations[DefaultLanguage];
    }
}
