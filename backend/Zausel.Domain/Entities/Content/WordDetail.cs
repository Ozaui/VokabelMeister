using Zausel.Domain.Entities;

namespace Zausel.Domain.Entities.Content;

// Dile özel gramer — 1:1 Word. GrammarData'nın şekli Word.LanguageId'ye göre değişir
// (GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9).
public class WordDetail : BaseEntity
{
    public int WordId { get; set; }
    public string? Pronunciation { get; set; }
    public string? Notes { get; set; }
    public string? CommonMistakes { get; set; }
    public string? GrammarData { get; set; }
}
