// ─────────────────────────────────────────────────────────────────────────────
// LocalizedMessageResolver.cs
//
// AMAÇ: Kod→dil→metin sözlüğünden, istenen dile (bulunamazsa varsayılana) karşılık
//       gelen metni çözen paylaşılan algoritma.
// NEDEN: ErrorMessages.cs ve SuccessMessages.cs birebir aynı `Resolve` mantığını
//        (yalnızca sözlük içerikleri farklı) bağımsız olarak taşıyordu (kod
//        denetiminde bulunan DRY ihlali) — iki sözlük ayrı kalmaya devam ediyor
//        (kodları anlamca farklı kümeler, bkz. SuccessMessages.cs dosya başı),
//        yalnızca çözümleme algoritması burada birleşti.
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

internal static class LocalizedMessageResolver
{
    // AMAÇ: Bir koda, istenen dile (bulunamazsa varsayılan dile) karşılık gelen metni döner.
    // NEDEN: Sözlükte olmayan bir kod gelirse (programlama hatası — yeni bir kod eklenip
    //        çevirisi eklenmemişse) exception fırlatmak yerine kodun kendisi döner; API
    //        asla yalnızca çeviri eksik diye 500'e düşmemeli.
    public static string Resolve(
        IReadOnlyDictionary<string, Dictionary<string, string>> messages,
        string code,
        string? language,
        string defaultLanguage
    )
    {
        if (!messages.TryGetValue(code, out var translations))
            return code;

        var lang = string.IsNullOrWhiteSpace(language) ? defaultLanguage : language;
        return translations.TryGetValue(lang, out var message) ? message : translations[defaultLanguage];
    }
}
