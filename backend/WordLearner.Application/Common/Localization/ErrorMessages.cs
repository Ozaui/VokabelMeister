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
        ["GECERSIZ_KIMLIK"] = new()
        {
            ["tr"] = "E-posta veya şifre hatalı.",
            ["de"] = "E-Mail oder Passwort ist falsch.",
        },
        ["GECERSIZ_OTP"] = new()
        {
            ["tr"] = "Girilen kod geçersiz veya süresi dolmuş.",
            ["de"] = "Der eingegebene Code ist ungültig oder abgelaufen.",
        },
        ["EPOSTA_KAYITLI"] = new()
        {
            ["tr"] = "Bu e-posta adresi zaten kayıtlı.",
            ["de"] = "Diese E-Mail-Adresse ist bereits registriert.",
        },
        ["HESAP_DONDURULMUS"] = new()
        {
            ["tr"] = "Hesabınız dondurulmuş. Lütfen destekle iletişime geçin.",
            ["de"] = "Ihr Konto wurde gesperrt. Bitte wenden Sie sich an den Support.",
        },
        ["HESAP_SILINMIS"] = new()
        {
            ["tr"] = "Bu hesap kalıcı olarak silinmiş.",
            ["de"] = "Dieses Konto wurde dauerhaft gelöscht.",
        },
        ["GECERSIZ_REFRESH_TOKEN"] = new()
        {
            ["tr"] = "Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.",
            ["de"] = "Ihre Sitzung ist abgelaufen. Bitte melden Sie sich erneut an.",
        },
        ["GECERSIZ_SOSYAL_TOKEN"] = new()
        {
            ["tr"] = "Sosyal giriş doğrulanamadı.",
            ["de"] = "Die soziale Anmeldung konnte nicht verifiziert werden.",
        },

        // NEDEN bu blok: FluentValidation validator'ları (Application/Validators/Auth/)
        //       WithMessage() ile yalnızca sabit Türkçe bir LOG mesajı taşır — istemciye
        //       giden gerçek mesaj, her kuralın WithErrorCode() ile taşıdığı bu kodlar
        //       üzerinden ValidationFilter tarafından buradan çözülür (AppException ile
        //       birebir aynı ayrım: log=sabit Türkçe, API yanıtı=dile göre).
        ["EMAIL_ZORUNLU"] = new()
        {
            ["tr"] = "E-posta adresi zorunludur.",
            ["de"] = "E-Mail-Adresse ist erforderlich.",
        },
        ["EMAIL_GECERSIZ"] = new()
        {
            ["tr"] = "Geçerli bir e-posta adresi girin.",
            ["de"] = "Geben Sie eine gültige E-Mail-Adresse ein.",
        },
        ["SIFRE_ZORUNLU"] = new()
        {
            ["tr"] = "Şifre zorunludur.",
            ["de"] = "Passwort ist erforderlich.",
        },
        ["SIFRE_KISA"] = new()
        {
            ["tr"] = "Şifre en az 12 karakter olmalı.",
            ["de"] = "Das Passwort muss mindestens 12 Zeichen lang sein.",
        },
        ["SIFRE_BUYUK_HARF_EKSIK"] = new()
        {
            ["tr"] = "Şifre en az 1 büyük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Großbuchstaben enthalten.",
        },
        ["SIFRE_KUCUK_HARF_EKSIK"] = new()
        {
            ["tr"] = "Şifre en az 1 küçük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Kleinbuchstaben enthalten.",
        },
        ["SIFRE_RAKAM_EKSIK"] = new()
        {
            ["tr"] = "Şifre en az 1 rakam içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Ziffer enthalten.",
        },
        ["SIFRE_OZEL_KARAKTER_EKSIK"] = new()
        {
            ["tr"] = "Şifre en az 1 özel karakter içermeli (!@#$%^&*).",
            ["de"] = "Das Passwort muss mindestens 1 Sonderzeichen enthalten (!@#$%^&*).",
        },
        ["OTP_ZORUNLU"] = new()
        {
            ["tr"] = "Doğrulama kodu zorunludur.",
            ["de"] = "Bestätigungscode ist erforderlich.",
        },
        ["OTP_FORMAT_GECERSIZ"] = new()
        {
            ["tr"] = "Doğrulama kodu 6 haneli olmalı.",
            ["de"] = "Der Bestätigungscode muss 6-stellig sein.",
        },
        ["AD_ZORUNLU"] = new() { ["tr"] = "Ad zorunludur.", ["de"] = "Vorname ist erforderlich." },
        ["SOYAD_ZORUNLU"] = new()
        {
            ["tr"] = "Soyad zorunludur.",
            ["de"] = "Nachname ist erforderlich.",
        },
        ["TOKEN_ZORUNLU"] = new()
        {
            ["tr"] = "Token zorunludur.",
            ["de"] = "Token ist erforderlich.",
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
