// ─────────────────────────────────────────────────────────────────────────────
// SecurityHeadersMiddleware.cs
//
// AMAÇ: Her HTTP yanıtına OWASP tarafından önerilen standart güvenlik başlıklarını ekler.
// NEDEN: REFERENCE/SECURITY.md §5 — clickjacking (X-Frame-Options), MIME sniffing
//        (X-Content-Type-Options), referrer sızıntısı ve XSS gibi tarayıcı taraflı
//        saldırıları middleware seviyesinde, her endpoint'te tekrar yazmadan engeller.
// BAĞIMLILIKLAR: Yok — yalnızca ASP.NET Core HttpContext.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    // AMAÇ: Yanıt başlıklarını, gövde yazılmadan önce ekleyip pipeline'ı devam ettirir.
    // NEDEN: Başlıklar response başlamadan (headers gönderilmeden) eklenmelidir;
    //        bu yüzden _next(context) çağrısından ÖNCE set edilir.
    public async Task InvokeAsync(HttpContext context)
    {
        // NEDEN: Sayfanın başka bir sitede <iframe> içine alınmasını (clickjacking) engeller.
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // NEDEN: Tarayıcının Content-Type'ı görmezden gelip dosyayı "tahmin ederek"
        //        çalıştırmasını (MIME sniffing) engeller.
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // NEDEN: Farklı origin'e giden isteklerde tam URL yerine yalnızca origin'i gönderir;
        //        query string'deki hassas verilerin (token, id) sızmasını azaltır.
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // NEDEN: Yalnızca kendi origin'inden script/stil yüklenmesine izin vererek
        //        XSS ile enjekte edilen dış kaynaklı script'leri engeller.
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";

        // NEDEN: Uygulama konum/mikrofon/kamera kullanmadığı için tarayıcı bu
        //        API'lere erişimi baştan reddeder (izin istenemez).
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}
