// ─────────────────────────────────────────────────────────────────────────────
// ExceptionHandlingMiddleware.cs
//
// AMAÇ: İstek hattında (pipeline) yakalanmayan tüm exception'ları tek yerden
//       yakalayıp standart ApiErrorResponse JSON'ına çevirmek.
// NEDEN: Controller'larda try/catch tekrar etmeyi önler; her hatanın HTTP durum
//        kodu ve JSON şekli tek bir dosyadan yönetilir (REFERENCE/API_ENDPOINTS.md §1).
// BAĞIMLILIKLAR: WordLearner.Application.Common.Exceptions.EntityNotFoundException/AppException,
//                WordLearner.Application.Common.Localization.ErrorMessages,
//                WordLearner.Application.Common.Models.ApiErrorResponse,
//                WordLearner.API.Common.RequestLanguageResolver.
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: Pipeline'daki bir sonraki middleware'i ve loglama servisini enjekte eder.
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    // AMAÇ: İsteği bir sonraki middleware'e iletir; sırasında fırlatılan her
    //       exception'ı yakalayıp standart hata yanıtına çevirir.
    // NEDEN: try/catch burada tek bir noktada olduğu için controller'lar yalnızca
    //        "mutlu yol" veya bilinen iş kuralı exception'larına (EntityNotFoundException
    //        vb.) odaklanır; beklenmeyen her şey burada 500'e düşer.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // NEDEN: CODING_STANDARDS.md §1 — tüm log mesajları İngilizce; exception detayı
            //        (stack trace) yalnızca ApplicationLog'a (A-04'te DB sink) gider, istemciye asla.
            _logger.LogError(
                ex,
                "Unhandled exception caught: {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            await WriteErrorResponseAsync(context, ex);
        }
    }

    // AMAÇ: Exception tipine göre HTTP durum kodu + hata kodu eşlemesi yapıp
    //       ApiErrorResponse'u JSON olarak yanıta yazar.
    // NEDEN: EntityNotFoundException bilinen bir iş kuralı olduğu için 404 + kendi
    //        (dinamik entity adı içeren) mesajıyla döner. AppException'dan türeyen
    //        Auth vb. exception'ların istemciye giden mesajı ise ex.Message DEĞİL —
    //        Code'un isteğin diline (Accept-Language) göre ErrorMessages'ten çözülmüş
    //        hâlidir (bkz. AppException.cs NEDEN açıklaması). Bilinmeyen her exception'ın
    //        gerçek mesajı istemciye sızdırılmaz (bilgi ifşası riski) — sabit, güvenli
    //        bir mesaj döner.
    private static Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code) = ex switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND"),
            AppException appEx => (StatusCodeFor(appEx), appEx.Code),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR"),
        };

        // NEDEN: EntityNotFoundException'ın mesajı dinamiktir (entity adı içerir),
        //        doğrudan ex.Message kullanılır. AppException'lar Code üzerinden
        //        isteğin diline göre çözülür. INTERNAL_SERVER_ERROR ise hiçbir zaman
        //        gerçek exception mesajını sızdırmaz, sabit bir metin döner.
        var message = ex switch
        {
            EntityNotFoundException => ex.Message,
            AppException appEx => ErrorMessages.Resolve(appEx.Code, RequestLanguageResolver.Resolve(context)),
            _ => "Beklenmeyen bir hata oluştu.",
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

    // AMAÇ: Her AppException alt tipini kendi HTTP durum koduna eşler.
    // NEDEN: Kod (Code) ile HTTP durumu ayrı kavramlar — aynı Code farklı senaryoda
    //        farklı statü dönebilir ihtimaline karşı, statü ayrı bir switch'te tutulur.
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
            _ => HttpStatusCode.BadRequest,
        };
}
