namespace WordLearner.Domain.Entities.Words;

// Bir WordConcept'in tek bir dildeki karşılığı (ör. "Tisch" de, "masa" tr) — her dil ayrı
// satır, farklı zamanlarda ayrı girilip sonradan eşleştirilebilir (Icerik.md "Eşleştirme").
public class Word : BaseEntity
{
    public int WordConceptId { get; set; }
    public WordConcept WordConcept { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    // Serbest anlam notu (genelde karşı dilde kısa gloss) — kartta gösterilen resmi çeviri
    // DEĞİL (o eşleşen Word.Text'ten gelir), eşleştirme ipucu olarak kullanılır.
    public string? Definition { get; set; }

    public bool IsActive { get; set; } = true;

    public WordDetail? WordDetail { get; set; }
    public ICollection<WordExample> WordExamples { get; set; } = new List<WordExample>();
}
