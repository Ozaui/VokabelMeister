namespace WordLearner.Application.Common.Localization;

public record EmailContent(string Subject, string HtmlBody);

// E-posta gövdesi de istemciye giden bir metin — ErrorMessages/SuccessMessages ile aynı
// "koda göre anahtarla, dile göre çöz" deseni; yalnızca değer tek satır yerine konu + HTML gövde.
public static class EmailTemplates
{
    private const string DefaultLanguage = "tr";

    private const string BrandColor = "#6D5DFC";
    private const string TextColor = "#1E1B2E";
    private const string MutedTextColor = "#6B7280";

    private static readonly Dictionary<string, Dictionary<string, EmailContent>> Templates = new()
    {
        ["EMAIL_VERIFICATION"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — E-posta Doğrulama",
                Layout(
                    "E-posta adresinizi doğrulayın",
                    "<p>VokabelMeister'a hoş geldiniz! Hesabınızı etkinleştirmek için aşağıdaki doğrulama kodunu uygulamaya girin.</p>",
                    otpBlock: true,
                    "<p>Kod {1} dakika geçerlidir. Bu kaydı siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — E-Mail-Bestätigung",
                Layout(
                    "Bestätigen Sie Ihre E-Mail-Adresse",
                    "<p>Willkommen bei VokabelMeister! Geben Sie den folgenden Bestätigungscode in der App ein, um Ihr Konto zu aktivieren.</p>",
                    otpBlock: true,
                    "<p>Der Code ist {1} Minuten gültig. Falls Sie sich nicht registriert haben, können Sie diese E-Mail ignorieren.</p>"
                )
            ),
        },
        ["LOGIN_OTP"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — Giriş Kodu",
                Layout(
                    "Giriş doğrulama kodunuz",
                    "<p>Hesabınıza giriş yapmak için aşağıdaki kodu uygulamaya girin.</p>",
                    otpBlock: true,
                    "<p>Kod {1} dakika geçerlidir. Bu girişi siz denemediyseniz şifrenizi hemen değiştirin.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — Anmeldecode",
                Layout(
                    "Ihr Anmeldebestätigungscode",
                    "<p>Geben Sie den folgenden Code in der App ein, um sich bei Ihrem Konto anzumelden.</p>",
                    otpBlock: true,
                    "<p>Der Code ist {1} Minuten gültig. Falls Sie diese Anmeldung nicht versucht haben, ändern Sie sofort Ihr Passwort.</p>"
                )
            ),
        },
        ["PASSWORD_RESET"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — Şifre Sıfırlama",
                Layout(
                    "Şifrenizi sıfırlayın",
                    "<p>Şifrenizi sıfırlamak için aşağıdaki kodu uygulamaya girin.</p>",
                    otpBlock: true,
                    "<p>Kod {1} dakika geçerlidir. Bu isteği siz yapmadıysanız hesabınız güvendedir, bu e-postayı yok sayabilirsiniz.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — Passwort zurücksetzen",
                Layout(
                    "Setzen Sie Ihr Passwort zurück",
                    "<p>Geben Sie den folgenden Code in der App ein, um Ihr Passwort zurückzusetzen.</p>",
                    otpBlock: true,
                    "<p>Der Code ist {1} Minuten gültig. Falls Sie diese Anfrage nicht gestellt haben, ist Ihr Konto sicher und Sie können diese E-Mail ignorieren.</p>"
                )
            ),
        },
        ["ACCOUNT_DELETION"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — Hesap Silme Onayı",
                Layout(
                    "Hesap silme isteğinizi onaylayın",
                    "<p>Hesabınızı silmek istediğinizi onaylamak için aşağıdaki kodu uygulamaya girin.</p>",
                    otpBlock: true,
                    "<p>Kod {1} dakika geçerlidir. Silme işleminden sonra 30 gün içinde tekrar giriş yaparak hesabınızı geri alabilirsiniz. Bu isteği siz yapmadıysanız şifrenizi hemen değiştirin.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — Bestätigung der Kontolöschung",
                Layout(
                    "Bestätigen Sie Ihre Löschanfrage",
                    "<p>Geben Sie den folgenden Code in der App ein, um die Löschung Ihres Kontos zu bestätigen.</p>",
                    otpBlock: true,
                    "<p>Der Code ist {1} Minuten gültig. Nach der Löschung können Sie Ihr Konto innerhalb von 30 Tagen durch erneutes Anmelden wiederherstellen. Falls Sie diese Anfrage nicht gestellt haben, ändern Sie sofort Ihr Passwort.</p>"
                )
            ),
        },
        ["PASSWORD_CHANGED"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — Şifreniz Değiştirildi",
                Layout(
                    "Şifreniz değiştirildi",
                    "<p>Hesabınızın şifresi az önce değiştirildi ve güvenlik gereği tüm cihazlardaki oturumlarınız kapatıldı.</p>",
                    otpBlock: false,
                    "<p>Bu değişikliği siz yapmadıysanız hemen şifrenizi sıfırlayın ve bizimle iletişime geçin.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — Ihr Passwort wurde geändert",
                Layout(
                    "Ihr Passwort wurde geändert",
                    "<p>Das Passwort Ihres Kontos wurde soeben geändert und aus Sicherheitsgründen wurden alle Sitzungen auf allen Geräten beendet.</p>",
                    otpBlock: false,
                    "<p>Falls Sie diese Änderung nicht vorgenommen haben, setzen Sie Ihr Passwort sofort zurück und kontaktieren Sie uns.</p>"
                )
            ),
        },
        ["ACCOUNT_RECOVERED"] = new()
        {
            ["tr"] = new(
                "VokabelMeister — Hesabınız Geri Alındı",
                Layout(
                    "Hesabınız geri alındı",
                    "<p>Silinmek üzere işaretlenmiş hesabınıza tekrar giriş yaptığınız için hesabınız geri alındı. Tüm kelimeleriniz ve ilerlemeniz korundu.</p>",
                    otpBlock: false,
                    "<p>Bu girişi siz yapmadıysanız şifrenizi hemen değiştirin.</p>"
                )
            ),
            ["de"] = new(
                "VokabelMeister — Ihr Konto wurde wiederhergestellt",
                Layout(
                    "Ihr Konto wurde wiederhergestellt",
                    "<p>Da Sie sich erneut bei Ihrem zur Löschung vorgemerkten Konto angemeldet haben, wurde es wiederhergestellt. Alle Ihre Vokabeln und Ihr Fortschritt sind erhalten geblieben.</p>",
                    otpBlock: false,
                    "<p>Falls Sie sich nicht angemeldet haben, ändern Sie sofort Ihr Passwort.</p>"
                )
            ),
        },
    };

    // args: OTP şablonlarında {0}=kod, {1}=geçerlilik dakikası; bilgilendirme şablonlarında boş.
    public static EmailContent Resolve(string code, string? language, params object[] args)
    {
        if (!Templates.TryGetValue(code, out var translations))
            throw new ArgumentException($"Unknown email template code: {code}", nameof(code));

        var lang = string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language;
        var template = translations.TryGetValue(lang, out var found)
            ? found
            : translations[DefaultLanguage];

        return args.Length == 0
            ? template
            : template with { HtmlBody = string.Format(template.HtmlBody, args) };
    }

    // Süslü parantez YOK — gövde string.Format'tan geçtiği için CSS blokları kaçış gerektirirdi;
    // bu yüzden tüm stiller inline attribute olarak yazılır.
    private static string Layout(string heading, string intro, bool otpBlock, string footer) =>
        $"""
        <div style="font-family:Segoe UI,Helvetica,Arial,sans-serif;background-color:#F8F7FC;padding:32px 16px;">
          <div style="max-width:520px;margin:0 auto;background-color:#FFFFFF;border:1px solid #E9E5F5;border-radius:12px;padding:32px;">
            <p style="margin:0 0 24px;font-size:20px;font-weight:700;color:{BrandColor};">VokabelMeister</p>
            <h1 style="margin:0 0 16px;font-size:22px;color:{TextColor};">{heading}</h1>
            <div style="font-size:15px;line-height:1.6;color:{TextColor};">{intro}</div>
            {(otpBlock ? OtpBlock : string.Empty)}
            <div style="font-size:13px;line-height:1.6;color:{MutedTextColor};">{footer}</div>
          </div>
        </div>
        """;

    // Kod, e-posta istemcilerinin telefon numarası sanıp link'e çevirmemesi için harf aralığı
    // açılmış düz metin olarak basılır.
    private const string OtpBlock =
        """
        <p style="margin:24px 0;padding:16px;background-color:#F8F7FC;border:1px solid #E9E5F5;border-radius:8px;text-align:center;font-size:30px;font-weight:700;letter-spacing:8px;color:#1E1B2E;">{0}</p>
        """;
}
