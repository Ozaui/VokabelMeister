namespace WordLearner.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Başlıklar response başlamadan (headers gönderilmeden) eklenmelidir — _next'ten önce set edilir.
        context.Response.Headers["X-Frame-Options"] = "DENY"; // clickjacking
        context.Response.Headers["X-Content-Type-Options"] = "nosniff"; // MIME sniffing
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}
