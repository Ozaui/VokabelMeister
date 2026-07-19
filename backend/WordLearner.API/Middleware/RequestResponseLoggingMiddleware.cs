// ─────────────────────────────────────────────────────────────────────────────
// RequestResponseLoggingMiddleware.cs
//
// AMAÇ: Gelen her HTTP isteğini ve verilen yanıtı (metot, yol, durum kodu, süre)
//       Serilog üzerinden loglamak.
// NEDEN: REFERENCE/SECURITY.md §6 — ApplicationLog kaynağı olan Serilog'un temel
//        istek/yanıt izini burada üretiriz; A-04'te DB sink'e bağlanınca bu satırlar
//        otomatik olarak ApplicationLogs tablosuna da düşer.
// NEDEN LogContext.PushProperty (A-04): ApplicationLogColumnOptions'ın "RequestPath"/
//        "UserId" ek kolonları Serilog'un STANDART özellikleri değil — bu middleware
//        onları scope bazlı enrichment ile üretiyor. RequestPath istek başında bilinir
//        (hemen pushlanır); UserId ise Authentication middleware bu middleware'den
//        SONRA çalıştığı için (Program.cs pipeline sırası) ancak _next(context)
//        tamamlandıktan SONRA context.User'dan okunabilir — bu yüzden yalnızca
//        "Request finished" log satırını saran dar bir scope'ta pushlanır.
// BAĞIMLILIKLAR: Microsoft.Extensions.Logging, Serilog.Context (Program.cs'te Serilog sağlayıcı olarak takılı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace WordLearner.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // AMAÇ: İsteğin başlangıcını, bitişini ve geçen süreyi (ms) loglar.
    // NEDEN: Stopwatch, exception fırlasa bile try/finally ile durdurulur; böylece
    //        hata durumunda da gerçek süre ve nihai durum kodu (ExceptionHandlingMiddleware
    //        tarafından set edilmiş 500 vb.) doğru şekilde loglanır.
    public async Task InvokeAsync(HttpContext context)
    {
        using var _ = LogContext.PushProperty("RequestPath", context.Request.Path.ToString());

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Request started: {Method} {Path}",
            context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // NEDEN int? olarak pushlanır (string değil): Serilog claim'i doğrudan
            //       string olarak pushlarsa MSSqlServer sink'i Int kolonuna yazarken
            //       string→int dönüşümüne güvenmek zorunda kalır; burada elle parse
            //       edip gerçek bir sayısal değer vermek bu riski ortadan kaldırır.
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
            using (LogContext.PushProperty("UserId", userId))
            {
                _logger.LogInformation("Request finished: {Method} {Path} → {StatusCode} ({ElapsedMs}ms)",
                    context.Request.Method, context.Request.Path,
                    context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
