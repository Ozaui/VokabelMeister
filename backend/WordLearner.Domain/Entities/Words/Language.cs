namespace WordLearner.Domain.Entities.Words;

// Desteklenen dillerin (şu an de/tr) referans listesi — BaseEntity'den TÜREMEZ, sabit/seed veri.
public class Language
{
    public int Id { get; set; }

    // ISO 639-1 (ör. "de", "tr").
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
