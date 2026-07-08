# AppException

**Özet:** Bilinen iş kuralı hatalarının (Auth: `DuplicateEmailException`, `InvalidCredentialsException`, `InvalidOtpException`, `AccountNotActiveException`, `AccountAnonymizedException`, `InvalidRefreshTokenException`, `InvalidSocialTokenException`; QR ile Giriş — A-03.1: `QrSessionGoneException`(410), `QrSessionForbiddenException`(403)) türediği taban sınıf. Mesajı kendi içinde SABİTLEMEZ — yalnızca dilden bağımsız bir `Code` (ör. `INVALID_CREDENTIALS`) taşır. İstemciye giden gerçek mesaj, isteğin `Accept-Language` header'ına göre [[ErrorMessages]] sözlüğünden [[Middleware|ExceptionHandlingMiddleware]] tarafından çözülür.
**Kütüphaneler:** Yok — saf C#.
**Bağlantılar:** [[ErrorMessages]] · [[Middleware]] · [[ApiErrorResponse]] · [[EntityNotFoundException]] · [[Guvenlik_Politikalari]] · [[Auth_Domain]]

## Konum
`backend/WordLearner.Application/Common/Exceptions/AppException.cs`

## Neden İki Ayrı Mesaj Kanalı Var
```csharp
public abstract class AppException : Exception
{
    public string Code { get; }
    protected AppException(string code, string developerMessage) : base(developerMessage) { Code = code; }
}
```
- `.Message` (taban `Exception.Message`) → **yalnızca log/geliştirici** içindir, sabit Türkçe kalır
  (`REFERENCE/CODING_STANDARDS.md §1` kuralı). İstemciye ASLA gönderilmez.
- `Code` → [[ErrorMessages]].`Resolve(code, dil)` ile isteğin diline göre çözülen, **istemciye giden**
  metnin anahtarıdır.

Örnek somut exception (hepsi parametresiz, sabit Code taşır):
```csharp
public class InvalidCredentialsException : AppException
{
    public InvalidCredentialsException()
        : base("INVALID_CREDENTIALS", "Login denemesi: kimlik bilgileri geçersiz.") { }
}
```

## Middleware Akışı
1. Bir Command Handler (ör. `LoginCommandHandler`) → `throw new InvalidCredentialsException()`
2. [[Middleware]] → `StatusCodeFor(appEx)` ile Code'a göre HTTP durumu belirler (ör. 401)
3. [[Middleware]] → isteğin `Accept-Language` header'ından dili çıkarır (`GetRequestLanguage`)
4. [[ErrorMessages]].`Resolve(code, dil)` → sözlükten metni döner, dil yoksa `tr`'ye düşer
5. [[ApiErrorResponse]] `{ code, message }` olarak JSON'a yazılır

## Neden EntityNotFoundException Bundan Türemiyor
[[EntityNotFoundException]]'ın mesajı dinamiktir (entity adı + Id içerir) — sabit kod+sözlük modeline
uymaz, bu yüzden bilinçli olarak ayrı bırakıldı; 404 yanıtı hâlâ doğrudan `ex.Message` kullanır.

## Kararın Kökeni
Kullanıcı, Auth exception'larının hem log hem API yanıtında aynı sabit Türkçe metni kullanmasının,
ileride farklı dil tercihi olan kullanıcılara (uygulama şu an DE-TR kelime içeriğine odaklı ama
Mobil'de `i18next` zaten planlı, `REFERENCE/TECHNICAL_SPECIFICATIONS.md §2`) yanlış dilde hata
göstereceğini fark etti. Tartışılan üç seçenekten (log de dile göre değişsin / sabit İngilizce log /
sabit Türkçe log + ayrı dilde API yanıtı) **üçüncüsü** seçildi — loglar geliştirici ekip içindir,
isteği atan kullanıcının diline göre değişmesi debug'ı zorlaştırırdı.
