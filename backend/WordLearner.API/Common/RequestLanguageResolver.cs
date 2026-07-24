namespace WordLearner.API.Common;

public static class RequestLanguageResolver
{
    // "Accept-Language: de-DE,de;q=0.9" gibi bir header'dan "de" çıkarır. Header yoksa/parse
    // edilemezse null döner — ErrorMessages.Resolve bu durumda varsayılan Türkçe'ye düşer.
    public static string? Resolve(HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
            return null;

        var firstLanguage = header.Split(',')[0].Split(';')[0].Trim();
        return firstLanguage.Length >= 2 ? firstLanguage[..2].ToLowerInvariant() : null;
    }
}
