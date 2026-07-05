// ─────────────────────────────────────────────────────────────────────────────
// RequestLanguageResolver.cs
//
// AMAÇ: HTTP isteğinin Accept-Language header'ından ilk dil kodunu çıkaran
//       paylaşılan yardımcı.
// NEDEN: ExceptionHandlingMiddleware VE ValidationFilter aynı mantığa ihtiyaç
//        duyuyordu (ikisi de ErrorMessages.Resolve'a hangi dilin isteneceğini
//        söylemeli) — kod tekrarını önlemek için tek yere alındı.
// BAĞIMLILIKLAR: Yok — saf ASP.NET Core HttpContext.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.API.Common;

public static class RequestLanguageResolver
{
    // AMAÇ: "Accept-Language: de-DE,de;q=0.9" gibi bir header'dan "de" çıkarır.
    // NEDEN: Header yoksa/parse edilemezse null döner; ErrorMessages.Resolve bu
    //        durumda varsayılan Türkçe'ye düşer (proje Türkçe öncelikli).
    public static string? Resolve(HttpContext context)
    {
        var header = context.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
            return null;

        var firstLanguage = header.Split(',')[0].Split(';')[0].Trim();
        return firstLanguage.Length >= 2 ? firstLanguage[..2].ToLowerInvariant() : null;
    }
}
