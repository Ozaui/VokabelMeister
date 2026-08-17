using System.Diagnostics;
using Serilog.Context;

namespace Zausel.API.Middleware;

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
        var stopwatch = Stopwatch.StartNew();

        // İsteğin path'ini, bu istek sırasında yazılan HER log satırına (Handler'lardaki dahil)
        // otomatik ekler — ApplicationLogs.RequestPath (A-04) bu sayede elle geçirilmeden dolar.
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        {
            await _next(context);
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
