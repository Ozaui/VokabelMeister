namespace WordLearner.Domain.Entities.Words;

public class WordExample : BaseEntity
{
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;

    public string SentenceText { get; set; } = string.Empty;

    // A1-C2.
    public string Level { get; set; } = "A1";

    // Normal/Idiom/Formal/Colloquial.
    public string ExampleType { get; set; } = "Normal";

    // Karşı dildeki çeviri (varsa) — NULL ise BAĞIMSIZDIR, "çeviri" değil ayrı bir cümledir.
    public int? PairedExampleId { get; set; }
    public WordExample? PairedExample { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
