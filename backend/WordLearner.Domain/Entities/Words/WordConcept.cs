// ─────────────────────────────────────────────────────────────────────────────
// WordConcept.cs
//
// AMAÇ: Dilden bağımsız bir "kelime kavramı" — kategori/seviye burada, her dildeki
//       karşılığı ayrı bir Word satırıdır (bkz. DATABASE_SCHEMA/Icerik.md).
// NEDEN: Çoklu dil desteği (şu an de+tr, ileride en) tek bir "kelime" tablosuna
//        dile özel kolon (NameEN, GermanWord vb.) eklemek yerine kavram + dil-başına-
//        satır modeliyle çözülür — CLAUDE.md "Çoklu dil" kuralı. Bir kavram tek
//        dilde Word'e sahipse "eşleşmemiş" sayılır (ayrı bir IsMatched kolonu
//        açılmadı — bkz. Icerik.md "Eşleştirme").
// BAĞIMLILIKLAR: BaseEntity, Word (1:N — her dil için bir satır), WordCategory (1:N — A-06).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Domain.Entities.Words;

public class WordConcept : BaseEntity
{
    // AMAÇ: Kelime türü — geçerli değerler: Noun, Verb, Adjective, Adverb, Conjunction,
    //       Preposition, Pronoun, Other.
    // NEDEN: Enum yerine string tutulur — User.Role/CurrentLevel'daki gibi asıl
    //        savunma DB CHECK constraint'inde; WordGrammarValidator'ın dile göre
    //        dallanan matrisinde (GERMAN/TURKISH_LANGUAGE_FEATURES.md) bu değer
    //        doğrudan karşılaştırılıyor.
    public string PartOfSpeech { get; set; } = string.Empty;

    // AMAÇ: Kelimenin zorluk seviyesi — geçerli değerler: A1, A2, B1, B2, C1, C2.
    public string DifficultyLevel { get; set; } = string.Empty;

    // AMAÇ: Kavramı temsil eden görsel (dilden bağımsız — ör. "elma" resmi hem de hem tr'de aynı).
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // AMAÇ: Bu kavramın her dildeki karşılığı (0, 1 veya 2 satır — 1 satır = "eşleşmemiş").
    public ICollection<Word> Words { get; set; } = new List<Word>();

    // AMAÇ: Bu kavramın bağlı olduğu kategoriler (A-06 — Kategori API).
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();
}
