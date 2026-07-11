# SuccessMessages

**Özet:** Token dönmeyen Auth endpoint'lerinin (`MessageResponse`) `Code` alanının her dildeki karşılığını tutan merkezi statik sözlük — [[ErrorMessages]] ile birebir aynı desen, ama başarı tarafı için. A-03.2'de (2026-07-11) eklendi.
**Kütüphaneler:** Yok — saf C#.
**Bağlantılar:** [[ErrorMessages]] · [[WordLearner_Application]] · [[Guvenlik_Politikalari]]

## Konum
`backend/WordLearner.Application/Common/Localization/SuccessMessages.cs`

## Kod
```csharp
public static class SuccessMessages
{
    private const string DefaultLanguage = "tr";
    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new() { /* ... */ };

    public static string Resolve(string code, string? language)
    {
        if (!Messages.TryGetValue(code, out var translations)) return code;
        var lang = string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language;
        return translations.TryGetValue(lang, out var message) ? message : translations[DefaultLanguage];
    }
}
```

## Şu An Tanımlı Kodlar (tr + de)
`OTP_SENT`, `EMAIL_VERIFIED`, `VERIFICATION_CODE_SENT`, `PASSWORD_UPDATED`, `PASSWORD_RESET_OTP_SENT`,
`ACCOUNT_DELETION_OTP_SENT`, `ACCOUNT_DELETION_CONFIRMED` — her biri `MessageResponse` döndüren bir
Auth Command Handler'ın (`LoginCommand`, `VerifyEmailCommand`, `ResendVerificationCommand`,
`ResetPasswordCommand`, `ForgotPasswordCommand`, `RequestAccountDeletionCommand`,
`ConfirmAccountDeletionCommand`) ürettiği koda karşılık gelir.

## Neden ErrorMessages'a EKLENMEDİ, ayrı bir sözlük yazıldı
Kodlar anlamca farklı iki küme: [[ErrorMessages]]'taki `ACCOUNT_DELETED` zaten "bu hesap kalıcı
silinmiş, giriş REDDEDİLDİ" anlamına gelen bir HATA koduyken, buradaki `ACCOUNT_DELETION_CONFIRMED`
"silme isteğin BAŞARIYLA alındı" anlamına gelen bir BAŞARI kodu — aynı sözlükte tutulsalar isim
çakışması/anlam karışıklığı olurdu.

## MessageResponse ile İlişki
`MessageResponse` DTO'su A-03.2'den önce tek alanlıydı (`Message`) — Handler doğrudan sabit Türkçe
bir string üretiyordu. Artık `record MessageResponse(string Code, string Message)`: Handler `Code`'u
sabit üretir (ör. `"OTP_SENT"`), `Message` isteğin `Accept-Language`'ına göre
`SuccessMessages.Resolve(Code, Language)` ile çözülür — `ApiErrorResponse` içindeki
`ApiErrorDetail(Code, Message)` ile simetrik bir tasarım.

## Güvenli Varsayılan Davranış
Sözlükte olmayan bir kod gelirse (yeni bir başarı kodu eklenip çevirisi unutulmuşsa) exception
fırlatmaz — kodun kendisini döner (bkz. [[ErrorMessages]] ile aynı güvenli varsayılan).
