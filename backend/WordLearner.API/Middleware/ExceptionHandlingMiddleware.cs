// ─────────────────────────────────────────────────────────────────────────────
// ExceptionHandlingMiddleware.cs
//
// AMAÇ: İstek hattında (pipeline) yakalanmayan tüm exception'ları tek yerden
//       yakalayıp standart ApiErrorResponse JSON'ına çevirmek.
// NEDEN: Controller'larda try/catch tekrar etmeyi önler; her hatanın HTTP durum
//        kodu ve JSON şekli tek bir dosyadan yönetilir (REFERENCE/API_ENDPOINTS.md §1).
// BAĞIMLILIKLAR: WordLearner.Application.Common.Exceptions.EntityNotFoundException,
//                WordLearner.Application.Common.Models.ApiErrorResponse.
// ─────────────────────────────────────────────────────────────────────────────

using System.Net;
using System.Text.Json;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;

namespace WordLearner.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // AMAÇ: Pipeline'daki bir sonraki middleware'i ve loglama servisini enjekte eder.
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            // NEDEN: CODING_STANDARDS.md §1 — tüm log mesajları Türkçe; exception detayı
            //        (stack trace) yalnızca ApplicationLog'a (A-04'te DB sink) gider, istemciye asla.
            _logger.LogError(ex, "İşlenmemiş hata yakalandı: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    // AMAÇ: Exception tipine göre HTTP durum kodu + hata kodu + mesaj eşlemesi yapıp
    //       ApiErrorResponse'u JSON olarak yanıta yazar.
    // NEDEN: EntityNotFoundException bilinen bir iş kuralı olduğu için 404 + kendi
    //        mesajıyla döner; bilinmeyen her exception'ın gerçek mesajı istemciye
    //        sızdırılmaz (bilgi ifşası riski) — yerine sabit, güvenli bir mesaj döner.
    private static Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message) = ex switch
        {
            EntityNotFoundException => (HttpStatusCode.NotFound, "BULUNAMADI", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "SUNUCU_HATASI", "Beklenmeyen bir hata oluştu.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse(code, message);
        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}
