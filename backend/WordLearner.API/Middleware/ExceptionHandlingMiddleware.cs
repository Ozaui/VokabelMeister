using WordLearner.Application.Common;
using WordLearner.Application.DTOs;
using WordLearner.Domain.Exceptions;

namespace WordLearner.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found. Path: {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. Path: {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code)
    {
        var message = ErrorMessages.Resolve(code, ExtractLanguage(context));

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(false, new ApiErrorDetail(code, message)));
    }

    // "de-DE, tr;q=0.9" gibi bir header'dan yalnızca birincil dil alt etiketini (ilk "de") çıkarır.
    private static string? ExtractLanguage(HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
            return null;

        var primary = header.Split(',')[0].Split(';')[0].Trim();
        return primary.Split('-')[0].ToLowerInvariant();
    }
}
