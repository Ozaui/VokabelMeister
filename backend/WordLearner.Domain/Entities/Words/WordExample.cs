// ─────────────────────────────────────────────────────────────────────────────
// WordExample.cs
//
// AMAÇ: Bir Word için seviyeli örnek cümle(ler).
// NEDEN: PairedExampleId yalnızca gerçekten birlikte girilen veya admin'in elle
//        eşleştirdiği örnekleri bağlar — iki dilde ayrı girilen örnekler birbirinin
//        çevirisi olacağı garanti edilemediği için varsayılan olarak BAĞIMSIZDIR
//        (bkz. Icerik.md "WordExamples" notu).
// BAĞIMLILIKLAR: BaseEntity, Word (N:1), WordExample (self N:1, opsiyonel çeviri bağı).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Words;

public class WordExample : BaseEntity
{
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;

    // AMAÇ: Bu Word'ün dilinde tek örnek cümle.
    public string SentenceText { get; set; } = string.Empty;

    // AMAÇ: Örnek cümlenin seviyesi — geçerli değerler: A1, A2, B1, B2, C1, C2.
    public string Level { get; set; } = "A1";

    // AMAÇ: Örnek cümlenin türü — geçerli değerler: Normal, Idiom, Formal, Colloquial.
    public string ExampleType { get; set; } = "Normal";

    // AMAÇ: Karşı dildeki çeviri örneği (varsa). NULL ise bu örnek BAĞIMSIZDIR —
    //       "çeviri" değil, o kelimeyi kullanan ayrı bir cümledir.
    public int? PairedExampleId { get; set; }
    public WordExample? PairedExample { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
