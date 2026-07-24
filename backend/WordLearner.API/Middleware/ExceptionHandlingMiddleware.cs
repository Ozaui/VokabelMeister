using System.Net;
using System.Text.Json;
using WordLearner.API.Common;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Common.Models;

namespace WordLearner.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
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
        catch (Exception ex)
        {
            // Stack trace yalnızca ApplicationLog'a gider, istemciye asla.
            _logger.LogError(
                ex,
                "Unhandled exception caught: {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code) = ex switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND"),
            AppException appEx => (StatusCodeFor(appEx), appEx.Code),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR"),
        };

        // Gerçek exception mesajı hiçbir zaman sızdırılmaz — Code isteğin diline göre çözülür.
        var language = RequestLanguageResolver.Resolve(context);
        var message = ex switch
        {
            EntityNotFoundException => ex.Message,
            AppException appEx => ErrorMessages.Resolve(appEx.Code, language),
            _ => ErrorMessages.Resolve("INTERNAL_SERVER_ERROR", language),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse(code, message);
        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return context.Response.WriteAsync(json);
    }

    private static HttpStatusCode StatusCodeFor(AppException ex) =>
        ex switch
        {
            DuplicateEmailException => HttpStatusCode.Conflict,
            InvalidCredentialsException => HttpStatusCode.Unauthorized,
            InvalidOtpException => HttpStatusCode.BadRequest,
            AccountNotActiveException => HttpStatusCode.Forbidden,
            AccountAnonymizedException => HttpStatusCode.Forbidden,
            InvalidRefreshTokenException => HttpStatusCode.Unauthorized,
            InvalidSocialTokenException => HttpStatusCode.Unauthorized,
            QrSessionGoneException => HttpStatusCode.Gone,
            QrSessionForbiddenException => HttpStatusCode.Forbidden,
            DuplicateWordException => HttpStatusCode.Conflict,
            CategoryHasChildrenException => HttpStatusCode.Conflict,
            CategoryHasActiveWordsException => HttpStatusCode.Conflict,
            UnsupportedFileTypeException => HttpStatusCode.BadRequest,
            FileTooLargeException => HttpStatusCode.BadRequest,
            FileRequiredException => HttpStatusCode.BadRequest,
            SmtpSettingsNotConfiguredException => HttpStatusCode.BadRequest,
            SmtpPasswordRequiredException => HttpStatusCode.BadRequest,
            SmtpTestFailedException => HttpStatusCode.BadGateway,
            _ => HttpStatusCode.BadRequest,
        };
}
