using WordLearner.Domain.Entities.Words;

namespace WordLearner.Domain.Entities.Categories;

// WordConceptId kullanılır, WordId DEĞİL — kategori dilden bağımsız bir kavram özelliğidir
// ("Tisch"/"masa" aynı kavram, aynı kategoriye ait olmalı; Word bazlı ilişki bunu bozardı).
public class WordCategory : BaseEntity
{
    public int WordConceptId { get; set; }
    public WordConcept WordConcept { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int DisplayOrder { get; set; }
}
