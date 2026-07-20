// ─────────────────────────────────────────────────────────────────────────────
// Word.cs
//
// AMAÇ: Bir WordConcept'in tek bir dildeki karşılığı (ör. "Tisch" de, "masa" tr).
// NEDEN: Her dil ayrı bir satır olduğu için aynı kavram farklı zamanlarda/farklı
//        toplu import'larla ayrı ayrı girilip sonradan eşleştirilebilir
//        (bkz. Icerik.md "Eşleştirme").
// BAĞIMLILIKLAR: BaseEntity, WordConcept (N:1), Language (N:1), WordDetail (1:1), WordExample (1:N).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Words;

public class Word : BaseEntity
{
    public int WordConceptId { get; set; }
    public WordConcept WordConcept { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;

    // AMAÇ: Kelimenin kendisi (ör. "Tisch").
    public string Text { get; set; } = string.Empty;

    // AMAÇ: Serbest "anlam notu" — dili SABİT DEĞİL, pratikte çoğunlukla karşı dilde
    //       kısa gloss (ör. "aber" → "ama, fakat, ancak"). Kartta gösterilen "resmi
    //       çeviri" DEĞİLDİR (o eşleşen Word.Text'ten gelir); birincil işlevi ayrı
    //       girilen içerikte eşleştirme ipucu olmak (bkz. Icerik.md "Eşleştirme",
    //       suggestedMatchConceptId).
    public string? Definition { get; set; }

    public bool IsActive { get; set; } = true;

    public WordDetail? WordDetail { get; set; }
    public ICollection<WordExample> WordExamples { get; set; } = new List<WordExample>();
}
