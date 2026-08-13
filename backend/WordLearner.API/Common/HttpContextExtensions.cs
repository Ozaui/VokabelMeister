namespace WordLearner.API.Common;

// ExceptionHandlingMiddleware VE Controller'ların (ApiControllerBase) İKİSİNİN de ihtiyaç duyduğu
// "Accept-Language'dan dil çıkar" mantığı — tek yerde, tekrar yazılmaz.
public static class HttpContextExtensions
{
    // "de-DE, tr;q=0.9" gibi bir header'dan yalnızca birincil dil alt etiketini (ilk "de") çıkarır.
    public static string? GetLanguage(this HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
            return null;

        var primary = header.Split(',')[0].Split(';')[0].Trim();
        return primary.Split('-')[0].ToLowerInvariant();
    }
}
