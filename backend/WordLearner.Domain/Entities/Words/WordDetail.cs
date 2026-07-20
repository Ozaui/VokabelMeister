// ─────────────────────────────────────────────────────────────────────────────
// WordDetail.cs
//
// AMAÇ: Bir Word'e özel (dile göre şekli değişen) gramer/telaffuz bilgisi.
// NEDEN: 1:1 ilişki ayrı bir tabloya çıkarıldı çünkü GrammarData her dil+tür için
//        tamamen farklı bir JSON şekli taşıyor (bkz. GERMAN/TURKISH_LANGUAGE_FEATURES.md);
//        Word'ün kendisine gömülseydi o tablo dile özel olmayan alanlarla (Text,
//        Definition) karışırdı.
// BAĞIMLILIKLAR: BaseEntity, Word (1:1).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Words;

public class WordDetail : BaseEntity
{
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;

    // AMAÇ: IPA telaffuz gösterimi.
    public string? Pronunciation { get; set; }

    public string? AudioUrl { get; set; }

    // AMAÇ: Bileşik kelime notu vb. serbest metin — hangi PartOfSpeech'te doldurulacağı
    //       dile göre değişir (de: yalnızca Noun, tr: Noun+Verb — bkz. dil dosyaları).
    public string? Notes { get; set; }

    public string? CommonMistakes { get; set; }

    // AMAÇ: Dile ve türe göre şekli değişen gramer verisi (JSON) — bkz.
    //       GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9.
    public string? GrammarData { get; set; }
}
