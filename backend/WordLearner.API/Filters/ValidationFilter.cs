using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WordLearner.API.Common;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Common.Models;

namespace WordLearner.API.Filters;

// FluentValidation.AspNetCore paketi (otomatik ModelState entegrasyonu) kullanılmıyor —
// bu filter DI'a kayıtlı IValidator<T>'leri reflection ile bulup manuel çalıştırır.
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var language = RequestLanguageResolver.Resolve(context.HttpContext);

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                // e.ErrorMessage kullanılmaz (sabit İngilizce log metni) — e.ErrorCode üzerinden
                // ErrorMessages.Resolve ile isteğin diline göre çözülür.
                var messages = result
                    .Errors.Select(e => ErrorMessages.Resolve(e.ErrorCode, language))
                    .Distinct();
                var combinedMessage = string.Join(" ", messages);

                context.Result = new BadRequestObjectResult(
                    new ApiErrorResponse("INVALID_REQUEST", combinedMessage)
                );
                return;
            }
        }

        await next();
    }
}
