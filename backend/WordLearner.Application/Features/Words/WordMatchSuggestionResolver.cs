// ─────────────────────────────────────────────────────────────────────────────
// WordMatchSuggestionResolver.cs
//
// AMAÇ: Eşleşmemiş bir Word için, karşı dilin eşleşmemiş havuzunda
//       `suggestedMatchConceptId` önerisini üreten paylaşılan yardımcı.
// NEDEN: `Definition` virgülle ayrılmış birden fazla karşılık içerebilir (ör.
//        "ama, fakat, ancak") — Icerik.md "Eşleştirme" bölümüne göre bu TEK bir
//        string olarak değil, virgülle token'lara bölünüp her biri ayrı ayrı
//        denenir (yoksa hiç eşleşme bulunmaz). Karşılaştırma İKİ yönlü çalışır:
//        (a) adayın Definition token'ları ↔ havuzdaki Word.Text, (b) adayın
//        Text'i ↔ havuzdaki Word.Definition token'ları — "veya tersi" notu.
//        GetUnmatchedWordConceptsQueryHandler'ın tek tüketicisi.
// BAĞIMLILIKLAR: Word/WordConcept entity'leri.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public static class WordMatchSuggestionResolver
{
    // AMAÇ: `candidate` Word'üne karşı dilin eşleşmemiş havuzunda (`otherLanguagePool`,
    //       her biri tek Word'lü WordConcept) en iyi adayın Id'sini döner, yoksa null.
    // NASIL: Havuz sırasıyla taranır, ilk örtüşen aday döner (birden fazla eşanmalı
    //        Definition'da yalnızca biri eşleştirilebilir — Icerik.md "bilinçli sınırlama").
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
