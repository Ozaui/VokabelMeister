using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

// Definition virgülle ayrılmış birden fazla karşılık içerebilir (ör. "ama, fakat, ancak") — bu
// yüzden token'lara bölünüp iki yönlü karşılaştırılır: adayın Definition'ı ↔ havuzdaki Text,
// adayın Text'i ↔ havuzdaki Definition.
public static class WordMatchSuggestionResolver
{
    public static int? FindSuggestion(Word candidate, IReadOnlyList<WordConcept> otherLanguagePool)
    {
        var candidateTokens = SplitDefinition(candidate.Definition);

        foreach (var poolConcept in otherLanguagePool)
        {
            var poolWord = poolConcept.Words.Single();

            if (candidateTokens.Contains(poolWord.Text, StringComparer.OrdinalIgnoreCase))
                return poolConcept.Id;

            var poolTokens = SplitDefinition(poolWord.Definition);
            if (poolTokens.Contains(candidate.Text, StringComparer.OrdinalIgnoreCase))
                return poolConcept.Id;
        }

        return null;
    }

    private static string[] SplitDefinition(string? definition) =>
        string.IsNullOrWhiteSpace(definition)
            ? []
            : definition.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
