namespace WordLearner.Application.Common.Localization;

// ErrorMessages.cs ve SuccessMessages.cs aynı Resolve mantığını taşıyordu — sözlükler kodları
// anlamca farklı kümeler olduğu için ayrı kalır, yalnızca çözümleme algoritması burada birleşti.
internal static class LocalizedMessageResolver
{
    // Sözlükte olmayan bir kod gelirse (çevirisi eklenmemiş yeni bir kod) exception fırlatmak
    // yerine kodun kendisi döner; API yalnızca çeviri eksik diye 500'e düşmemeli.
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
