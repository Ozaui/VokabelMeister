using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Domain.Entities.Words;

// Dilden bağımsız "kelime kavramı" — kategori/seviye burada, her dildeki karşılığı ayrı
// bir Word satırı. Bir kavramın tek dilde Word'ü varsa "eşleşmemiş" sayılır.
public class WordConcept : BaseEntity
{
    // Noun/Verb/Adjective/... — enum değil string, asıl savunma DB CHECK constraint'inde;
    // WordGrammarValidator'ın dile göre dallanan matrisinde doğrudan karşılaştırılıyor.
    public string PartOfSpeech { get; set; } = string.Empty;

    // A1-C2.
    public string DifficultyLevel { get; set; } = string.Empty;

    // Dilden bağımsız (ör. "elma" resmi hem de hem tr'de aynı).
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Word> Words { get; set; } = new List<Word>();
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();
}
