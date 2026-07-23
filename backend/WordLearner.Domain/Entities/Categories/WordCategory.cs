// ─────────────────────────────────────────────────────────────────────────────
// WordCategory.cs
//
// AMAÇ: WordConcept ↔ Category çoka-çok (M:N) ara tablosu — bir kavram birden
//       fazla kategoriye, bir kategori birden fazla kavrama ait olabilir.
// NEDEN: WordConceptId kullanılır, WordId DEĞİL — kategori dilden bağımsız bir
//        kavram özelliğidir (ör. "Tisch" de "masa" tr AYNI kavram, ikisi de AYNI
//        "Ev" kategorisine ait olmalı); Word bazlı bir ilişki, iki dildeki aynı
//        kavramın FARKLI kategori setine düşmesine izin verirdi, bu da Icerik.md'nin
//        "kategori dilden bağımsız" kararına aykırı olurdu.
// BAĞIMLILIKLAR: BaseEntity, WordConcept (N:1), Category (N:1).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Words;

namespace WordLearner.Domain.Entities.Categories;

public class WordCategory : BaseEntity
{
    public int WordConceptId { get; set; }
    public WordConcept WordConcept { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // AMAÇ: Bir kategori içinde kelimelerin gösterim sırası (ör. admin panel elle sıralama).
    public int DisplayOrder { get; set; }
}
