using WordLearner.Domain.Entities;

namespace WordLearner.Domain.Entities.Content;

// Bir WordConcept'in tek dildeki karşılığı — Definition serbest anlam notu (bkz. Icerik.md), resmi
// çeviri DEĞİL.
public class Word : BaseEntity
{
    public int WordConceptId { get; set; }
    public int LanguageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public bool IsActive { get; set; } = true;
}
