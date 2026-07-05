// ─────────────────────────────────────────────────────────────────────────────
// RequestResponseLoggingMiddleware.cs
//
// AMAÇ: Gelen her HTTP isteğini ve verilen yanıtı (metot, yol, durum kodu, süre)
//       Serilog üzerinden loglamak.
// NEDEN: REFERENCE/SECURITY.md §6 — ApplicationLog kaynağı olan Serilog'un temel
//        istek/yanıt izini burada üretiriz; ileride A-04'te DB sink'e bağlanınca
//        bu satırlar otomatik olarak ApplicationLog tablosuna da düşer.
// BAĞIMLILIKLAR: Microsoft.Extensions.Logging (Serilog Program.cs'te sağlayıcı olarak takılı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Diagnostics;

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
            _logger.LogInformation("Request finished: {Method} {Path} → {StatusCode} ({ElapsedMs}ms)",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
