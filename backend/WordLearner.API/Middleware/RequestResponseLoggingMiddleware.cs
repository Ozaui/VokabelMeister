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

            // Authentication middleware bu middleware'den SONRA çalıştığı için (pipeline sırası)
            // UserId claim'i ancak _next(context) tamamlandıktan sonra okunabilir. int? olarak
            // pushlanır — Serilog'un MSSqlServer sink'i string→int dönüşümüne güvenmesin diye.
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
