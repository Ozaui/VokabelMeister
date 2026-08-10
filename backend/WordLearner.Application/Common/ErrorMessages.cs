namespace WordLearner.Application.Common;

// Yeni dil eklemek yalnızca bu sözlüğe yeni bir üst-seviye anahtar (ör. "en") eklemekle olur —
// var olan Code'lara veya exception sınıflarına dokunulmaz (CLAUDE.md §1).
public static class ErrorMessages
{
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        [DefaultLanguage] = new()
        {
            ["ENTITY_NOT_FOUND"] = "Kayıt bulunamadı.",
            ["INTERNAL_SERVER_ERROR"] = "Beklenmeyen bir hata oluştu.",
            ["ACCOUNT_ANONYMIZED"] = "Bu hesap kalıcı olarak silinmiş.",
        },
        ["de"] = new()
        {
            ["ENTITY_NOT_FOUND"] = "Eintrag nicht gefunden.",
            ["INTERNAL_SERVER_ERROR"] = "Ein unerwarteter Fehler ist aufgetreten.",
            ["ACCOUNT_ANONYMIZED"] = "Dieses Konto wurde dauerhaft anonymisiert.",
        },
    };

    public static string Resolve(string code, string? language)
    {
        var resolvedLanguage = language is not null && Messages.ContainsKey(language)
            ? language
            : DefaultLanguage;

        return Messages[resolvedLanguage].TryGetValue(code, out var message) ? message : code;
    }
}
