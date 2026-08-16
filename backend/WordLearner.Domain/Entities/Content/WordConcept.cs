using WordLearner.Domain.Entities;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Domain.Entities.Content;

// Dilden bağımsız kavram — kategori/seviye burada, dile özel metin Word'de.
public class WordConcept : BaseEntity
{
    public PartOfSpeech PartOfSpeech { get; set; }
    public string DifficultyLevel { get; set; } = "A1";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
