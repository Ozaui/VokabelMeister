# Middleware (ExceptionHandling / SecurityHeaders / RequestResponseLogging)

**Özet:** A-02'nin son ortak parçası — `WordLearner.API/Middleware/` altında [[Program_cs]]'in HTTP pipeline'ına taktığı 3 middleware sınıfı. `ExceptionHandlingMiddleware` yakalanmayan tüm exception'ları [[ApiErrorResponse]] JSON'ına çevirir; `SecurityHeadersMiddleware` her yanıta 5 güvenlik başlığı ekler; `RequestResponseLoggingMiddleware` her isteğin metot/yol/durum kodu/süresini Serilog ile loglar. Gerçek bir istekle (404 dahil) doğrulandı: başlıklar mevcut, Türkçe loglar konsola düşüyor.
**Kütüphaneler:** ASP.NET Core (`HttpContext`, `RequestDelegate`), Microsoft.Extensions.Logging (Serilog sağlayıcı olarak), `System.Text.Json`, `System.Diagnostics.Stopwatch`
**Bağlantılar:** [[Program_cs]] · [[EntityNotFoundException]] · [[AppException]] · [[ErrorMessages]] · [[ApiErrorResponse]] · [[Guvenlik_Politikalari]]

## Konum
`backend/WordLearner.API/Middleware/{ExceptionHandlingMiddleware,SecurityHeadersMiddleware,RequestResponseLoggingMiddleware}.cs`

## 1. ExceptionHandlingMiddleware
`try { await _next(context); } catch (Exception ex) { ... }` — `_next` çağrısını sarar, hatayı
Türkçe loglar (`_logger.LogError`) ve exception tipine göre eşler:

| Exception | HTTP Kodu | code | message |
|-----------|-----------|------|---------|
| [[EntityNotFoundException]] | 404 | `BULUNAMADI` | `ex.Message` (gerçek mesaj, dinamik — henüz çok dilli değil) |
| [[AppException]] alt tipleri (A-03) | `StatusCodeFor(appEx)` ile Code'a göre (401/403/409/400) | `appEx.Code` | [[ErrorMessages]].`Resolve(code, dil)` — `Accept-Language`'a göre |
| Diğer her şey | 500 | `SUNUCU_HATASI` | Sabit "Beklenmeyen bir hata oluştu." (gerçek mesaj **sızdırılmaz**) |

Yanıt [[ApiErrorResponse]] olarak `System.Text.Json` ile camelCase serileştirilir
(`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`). `GetRequestLanguage` yardımcı metodu
`Accept-Language` header'ının ilk 2 harfini (ör. `en-US`→`en`) çıkarır; header yoksa/tanınmıyorsa
[[ErrorMessages]] varsayılan Türkçe'ye düşer.

## 2. SecurityHeadersMiddleware
`_next(context)` çağrısından **önce** 5 başlığı ekler (yanıt başlamadan eklenmeli):

```
X-Frame-Options: DENY                                    → clickjacking engeli
X-Content-Type-Options: nosniff                          → MIME sniffing engeli
Referrer-Policy: strict-origin-when-cross-origin         → cross-origin referrer sızıntısı azaltma
Content-Security-Policy: default-src 'self'              → yalnızca kendi origin'inden script/stil
Permissions-Policy: geolocation=(), microphone=(), camera=()  → kullanılmayan API'lere baştan ret
```
Kaynak: [[Guvenlik_Politikalari]] §5.

## 3. RequestResponseLoggingMiddleware
`Stopwatch` ile isteğin süresini ölçer; `try/finally` kullanır ki bir exception fırlasa (ve
`ExceptionHandlingMiddleware` onu yakalayıp 500'e çevirse) bile `finally` bloğu çalışıp gerçek
süreyi ve nihai `context.Response.StatusCode`'u loglasın.
```
İstek başladı: {Method} {Path}
İstek bitti: {Method} {Path} → {StatusCode} ({ElapsedMs}ms)
```

## Pipeline Sıralaması (Program.cs)
```
RequestResponseLoggingMiddleware   ← en dışta (exception olsa bile süre/kod ölçülsün)
SecurityHeadersMiddleware
ExceptionHandlingMiddleware        ← en içte (her şeyi yakalar)
```

## İleride Bağlanacak
`RequestResponseLoggingMiddleware`'in ürettiği loglar, Serilog'un `ApplicationLog` DB sink'i A-04'te
eklenince otomatik olarak veritabanına da düşecek — bu dosyaya dokunmaya gerek yok (bkz. [[Loglama_Domain]]).
