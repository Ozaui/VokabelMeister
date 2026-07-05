// ─────────────────────────────────────────────────────────────────────────────
// ValidationFilter.cs
//
// AMAÇ: Controller action'lara gelen istek gövdesini (body), varsa DI'a kayıtlı
//       bir FluentValidation validator'ı ile doğrulayan, tüm controller'larda
//       ortak kullanılan global action filter.
// NEDEN: FluentValidation.AspNetCore paketi (otomatik ModelState entegrasyonu)
//        kullanılmıyor (REFERENCE/TECHNICAL_SPECIFICATIONS.md §1'de yok) — bu
//        filter aynı işi, DI'a kayıtlı IValidator<T>'leri reflection ile bulup
//        manuel çalıştırarak yapar; her controller action'ının validasyon
//        çağrısını elle yazmasına gerek kalmaz (ince katman kuralı, CODING_STANDARDS.md §5).
//        KRİTİK: İstemciye giden mesaj her validation hatasının ErrorMessage'ı
//        DEĞİL, ErrorCode'udur — ErrorMessage yalnızca validator'ın WithMessage()
//        ile verdiği sabit TÜRKÇE log/geliştirici açıklamasıdır (AppException.Message
//        ile birebir aynı ayrım). ErrorCode, ErrorMessages sözlüğünden isteğin
//        Accept-Language'ına göre çözülür — aksi hâlde hata mesajları hep Türkçe
//        kilitli kalırdı.
// BAĞIMLILIKLAR: FluentValidation, WordLearner.Application.Common.Localization.ErrorMessages,
//                WordLearner.Application.Common.Models.ApiErrorResponse.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WordLearner.API.Common;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Common.Models;

namespace WordLearner.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    // AMAÇ: Action çalışmadan önce, argümanlardan biri için DI'da kayıtlı bir
    //       IValidator<T> varsa çalıştırır; başarısızsa 400 döner ve action'ı
    //       hiç çalıştırmaz.
    // NEDEN: Tek bir yerden tüm controller'lar için doğrulama sağlanır — yeni bir
    //        controller/endpoint eklenince yalnızca ilgili Validator yazılır,
    //        bu filter'a dokunulmaz.
    // NASIL: 1) Her action argümanı için tipine uygun IValidator<T> ara
    //        2) Bulunursa ValidateAsync çağır  3) Geçersizse her hatanın
    //        ErrorCode'unu isteğin diline göre ErrorMessages'ten çözüp birleştir,
    //        400 + ApiErrorResponse dön  4) Geçerliyse bir sonraki argümana geç.
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
                // NEDEN: e.ErrorMessage KULLANILMAZ (sabit Türkçe log metni) — e.ErrorCode
                //        üzerinden ErrorMessages.Resolve ile isteğin diline göre çözülür.
                var messages = result
                    .Errors.Select(e => ErrorMessages.Resolve(e.ErrorCode, language))
                    .Distinct();
                var combinedMessage = string.Join(" ", messages);

                context.Result = new BadRequestObjectResult(
                    new ApiErrorResponse("GECERSIZ_ISTEK", combinedMessage)
                );
                return;
            }
        }

        await next();
    }
}
