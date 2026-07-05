// ─────────────────────────────────────────────────────────────────────────────
// ErrorMessages.cs
//
// AMAÇ: AppException.Code değerlerinin her dildeki karşılığını tutan merkezi sözlük.
// NEDEN: Exception'lar mesajı kendi içinde sabitlemez — aynı kod, isteğin diline
//        göre farklı bir metne çevrilebilsin diye (REFERENCE/API_ENDPOINTS.md §1).
//        Yeni bir dil eklemek (ör. de) yalnızca buraya bir sütun eklemekle olur,
//        hiçbir exception sınıfına dokunulmaz.
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

public static class ErrorMessages
{
    // NEDEN varsayılan "tr": İstekte Accept-Language yoksa veya bilinmeyen bir dilse,
    //        proje Türkçe öncelikli olduğu için Türkçe'ye düşülür (00_INDEX.md kuralı).
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages =
        new()
        {
            ["GECERSIZ_KIMLIK"] = new()
            {
                ["tr"] = "E-posta veya şifre hatalı.",
                ["en"] = "Invalid email or password.",
            },
            ["GECERSIZ_OTP"] = new()
            {
                ["tr"] = "Girilen kod geçersiz veya süresi dolmuş.",
                ["en"] = "The code you entered is invalid or has expired.",
            },
            ["EPOSTA_KAYITLI"] = new()
            {
                ["tr"] = "Bu e-posta adresi zaten kayıtlı.",
                ["en"] = "This email address is already registered.",
            },
            ["HESAP_DONDURULMUS"] = new()
            {
                ["tr"] = "Hesabınız dondurulmuş. Lütfen destekle iletişime geçin.",
                ["en"] = "Your account has been suspended. Please contact support.",
            },
            ["HESAP_SILINMIS"] = new()
            {
                ["tr"] = "Bu hesap kalıcı olarak silinmiş.",
                ["en"] = "This account has been permanently deleted.",
            },
            ["GECERSIZ_REFRESH_TOKEN"] = new()
            {
                ["tr"] = "Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.",
                ["en"] = "Your session has expired. Please sign in again.",
            },
            ["GECERSIZ_SOSYAL_TOKEN"] = new()
            {
                ["tr"] = "Sosyal giriş doğrulanamadı.",
                ["en"] = "Social sign-in could not be verified.",
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
        return translations.TryGetValue(lang, out var message) ? message : translations[DefaultLanguage];
    }
}
