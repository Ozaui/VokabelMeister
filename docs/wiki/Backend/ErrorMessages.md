# ErrorMessages

**Özet:** [[AppException]].`Code` değerlerinin her dildeki karşılığını tutan merkezi statik sözlük. Şu an tr/de dolu (hedef kitle DE↔TR). Yeni bir dil eklemek (ör. `en`, ileride) yalnızca buraya bir sütun eklemekle olur — hiçbir exception sınıfına dokunulmaz.
**Kütüphaneler:** Yok — saf C#.
**Bağlantılar:** [[AppException]] · [[Middleware]] · [[Guvenlik_Politikalari]] · [[SuccessMessages]]

## Başarı Tarafındaki Kardeşi
A-03.2'de (2026-07-11) [[SuccessMessages]] eklendi — birebir aynı `Resolve(code, language)` deseni,
ama `AppException.Code` yerine `MessageResponse.Code`'u çözer. Ayrı sözlük: kodlar anlamca farklı
kümeler (ör. `ACCOUNT_DELETED` burada bir HATA koduyken `SuccessMessages`'taki
`ACCOUNT_DELETION_CONFIRMED` bir BAŞARI kodudur).

## Konum
`backend/WordLearner.Application/Common/Localization/ErrorMessages.cs`

## Kod
```csharp
public static class ErrorMessages
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
`INVALID_CREDENTIALS`, `INVALID_OTP`, `EMAIL_ALREADY_REGISTERED`, `ACCOUNT_SUSPENDED`, `ACCOUNT_DELETED`,
`INVALID_REFRESH_TOKEN`, `INVALID_SOCIAL_TOKEN` (A-03) + `QR_SESSION_GONE`, `QR_SESSION_FORBIDDEN`
(A-03.1) — her biri bir [[AppException]] alt tipine karşılık gelir. + `INTERNAL_SERVER_ERROR`
(2026-07-11 — [[Middleware]]'in `AppException`'dan türemeyen exception'lar için kullandığı sabit
kod; öncesinde bu tek satır hardcoded Türkçe'ydi, artık diğerleriyle aynı `Resolve()` deseninden geçiyor).

## Güvenli Varsayılan Davranış
Sözlükte olmayan bir kod gelirse (yeni bir `AppException` eklenip çevirisi unutulmuşsa) exception
fırlatmaz — kodun kendisini döner. Böylece API, yalnızca çeviri eksik diye asla 500'e düşmez.
