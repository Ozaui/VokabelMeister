using WordLearner.API.Common;
using WordLearner.Application.Common;
using WordLearner.Application.Common.Exceptions;
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
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed: {Code}. Path: {Path}", ex.Errors.First().ErrorCode, context.Request.Path);
            await WriteValidationErrorAsync(context, ex.Errors);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Application exception: {Code}. Path: {Path}", ex.Code, context.Request.Path);
            await WriteErrorAsync(context, (int)ex.StatusCode, ex.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. Path: {Path}", context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code)
    {
        var message = ErrorMessages.Resolve(code, context.GetLanguage());

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(false, new ApiErrorDetail(code, message)));
    }

    // Birden fazla FluentValidation kuralı aynı anda başarısız olabilir — Details TÜMÜNÜ taşır,
    // Code/Message (ApiErrorDetail'in geri kalan istemcilerle PAYLAŞTIĞI alanlar) İLK kuralı taşır.
    private static async Task WriteValidationErrorAsync(HttpContext context, IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var language = context.GetLanguage();
        var details = failures
            .Select(f => new FieldError(ToCamelCase(f.PropertyName), f.ErrorCode, ErrorMessages.Resolve(f.ErrorCode, language)))
            .ToList();

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(false, new ApiErrorDetail(details[0].Code, details[0].Message, details)));
    }

    private static string ToCamelCase(string propertyName) =>
        propertyName.Length == 0 ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
