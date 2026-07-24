namespace WordLearner.Domain.Entities.Words;

// Word'e özel gramer/telaffuz bilgisi — ayrı tabloda çünkü GrammarData her dil+tür için
// tamamen farklı bir JSON şekli taşır (GERMAN/TURKISH_LANGUAGE_FEATURES.md).
public class WordDetail : BaseEntity
{
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;

    public string? Pronunciation { get; set; }
    public string? AudioUrl { get; set; }
    public string? Notes { get; set; }
    public string? CommonMistakes { get; set; }

    // Dile ve türe göre şekli değişen gramer verisi (JSON) — GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9.
    public string? GrammarData { get; set; }
}
