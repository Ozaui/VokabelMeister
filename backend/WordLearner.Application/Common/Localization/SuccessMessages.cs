// ─────────────────────────────────────────────────────────────────────────────
// SuccessMessages.cs
//
// AMAÇ: Token dönmeyen auth endpoint'lerinin (`MessageResponse`) `Code` alanının
//       her dildeki karşılığını tutan merkezi sözlük.
// NEDEN: `ErrorMessages.cs` ile birebir aynı desen — Handler mesajı kendi içinde
//        sabitlemez, isteğin diline göre farklı bir metne çevrilebilsin diye
//        (REFERENCE/SECURITY.md §1.4). Ayrı bir sözlük (ErrorMessages'a eklenmek
//        yerine): kodlar anlamca farklı kümeler — ör. "ACCOUNT_DELETED" zaten
//        ErrorMessages'ta "hesap kalıcı silinmiş, giriş reddedildi" hata koduyken,
//        buradaki "ACCOUNT_DELETION_CONFIRMED" "silme işlemin başarıyla alındı"
//        başarı kodu; aynı sözlükte tutulsalar isim çakışması/anlam karışıklığı
//        olurdu. Şu an yalnızca tr+de var (ErrorMessages.cs ile aynı YAGNI gerekçesi).
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

public static class SuccessMessages
{
    // NEDEN varsayılan "tr": bkz. ErrorMessages.cs — proje Türkçe öncelikli.
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
    };

    // AMAÇ: Bir başarı koduna, istenen dile (bulunamazsa Türkçe'ye) karşılık gelen mesajı döner.
    // NEDEN: bkz. LocalizedMessageResolver.Resolve — sözlükte olmayan bir kod gelirse
    //        (programlama hatası) exception fırlatmak yerine kodun kendisi döner.
    public static string Resolve(string code, string? language) =>
        LocalizedMessageResolver.Resolve(Messages, code, language, DefaultLanguage);
}
