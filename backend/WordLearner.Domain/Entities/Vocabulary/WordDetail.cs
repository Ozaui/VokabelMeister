/// <summary>
/// WordDetail.cs
///
/// AMAÇ:
///   Bir Almanca kelimenin dilbilgisel detaylarını saklar:
///   cinsiyet, 4 hâl artikelleri, tekil/çoğul formları, fiil çekimleri,
///   telaffuz (IPA) ve medya dosyaları.
///
/// NEDEN:
///   Words tablosunu şişirmemek için 1:1 ilişkiyle ayrı tutulur.
///   Gramer bilgisi her zaman gerekmez — listeleme sorgularında JOIN maliyeti ortadan kalkar.
///   ConjugationData JSON olarak saklanır çünkü fiil çekimleri çok sayıda zaman/kişi içerir.
///
/// ALMANCA DİLBİLGİSİ NOTU:
///   İsimler → cinsiyet (der/die/das) + 4 hâl (Nominativ, Akkusativ, Dativ, Genitiv) + çoğul
///   Fiiller → çekim tablosu (Präsens, Präteritum, Perfekt) + ayrılabilir ön ek
///   Referans: docs/GERMAN_LANGUAGE_FEATURES.md
///
/// BAĞIMLILIKLAR:
///   - Word (N:1 — bu entity'nin sahibi olan kelime)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Almanca gramer detay entity'si (1:1 → Word).
///
/// AMAÇ: Kelime kartı ekranında gösterilecek dilbilgisel bilgileri saklamak.
/// NEDEN BaseEntity'den miras almaz: Soft delete gerekmez — Word silinince CASCADE ile silinir.
/// </summary>
public class WordDetail
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Bağlı olduğu kelimenin ID'si (FK → Words, UNIQUE — 1:1 ilişki)</summary>
    public int WordId { get; set; }

    // ─── Cinsiyet ────────────────────────────────────────────────────────────

    /// <summary>
    /// Almanca cinsiyet: Masculine | Feminine | Neuter
    /// NEDEN NULL OLABİLİR: Fiiller ve bazı sözcük türlerinin cinsiyeti yoktur.
    /// </summary>
    public string? Gender { get; set; }

    // ─── Belirli Artikeller (4 Hâl) ─────────────────────────────────────────

    /// <summary>Belirli artikel — Nominativ: der / die / das</summary>
    public string? ArticleDefiniteNom { get; set; }

    /// <summary>Belirli artikel — Akkusativ: den / die / das</summary>
    public string? ArticleDefiniteAcc { get; set; }

    /// <summary>Belirli artikel — Dativ: dem / der / dem</summary>
    public string? ArticleDefiniteDat { get; set; }

    /// <summary>Belirli artikel — Genitiv: des / der / des</summary>
    public string? ArticleDefiniteGen { get; set; }

    // ─── Belirsiz Artikeller (4 Hâl) ────────────────────────────────────────

    /// <summary>Belirsiz artikel — Nominativ: ein / eine / ein</summary>
    public string? ArticleIndefiniteNom { get; set; }

    /// <summary>Belirsiz artikel — Akkusativ: einen / eine / ein</summary>
    public string? ArticleIndefiniteAcc { get; set; }

    /// <summary>Belirsiz artikel — Dativ: einem / einer / einem</summary>
    public string? ArticleIndefiniteDat { get; set; }

    /// <summary>Belirsiz artikel — Genitiv: eines / einer / eines</summary>
    public string? ArticleIndefiniteGen { get; set; }

    // ─── Tekil Hâl Formları (İsimler) ───────────────────────────────────────

    /// <summary>Nominativ tekil formu (genellikle sözlük formu)</summary>
    public string? FormNominative { get; set; }

    /// <summary>Akkusativ tekil formu</summary>
    public string? FormAccusative { get; set; }

    /// <summary>Dativ tekil formu</summary>
    public string? FormDative { get; set; }

    /// <summary>Genitiv tekil formu (genellikle -(e)s eklenir)</summary>
    public string? FormGenitive { get; set; }

    // ─── Çoğul Formlar ──────────────────────────────────────────────────────

    /// <summary>Temel çoğul formu (sözlük formu)</summary>
    public string? PluralForm { get; set; }

    /// <summary>Çoğul Nominativ</summary>
    public string? PluralFormNominative { get; set; }

    /// <summary>Çoğul Akkusativ</summary>
    public string? PluralFormAccusative { get; set; }

    /// <summary>Çoğul Dativ (genellikle Nominativ + -n)</summary>
    public string? PluralFormDative { get; set; }

    /// <summary>Çoğul Genitiv</summary>
    public string? PluralFormGenitive { get; set; }

    // ─── Fiil Çekimleri ──────────────────────────────────────────────────────

    /// <summary>
    /// Fiil çekim tablosu — JSON formatında (NVARCHAR(MAX)).
    ///
    /// NEDEN JSON: Zamanlar (Präsens, Präteritum, Perfekt) ve kişiler (ich, du, er…)
    ///             matrisi çok sayıda kolon açmak yerine tek alanda saklanır.
    ///
    /// JSON YAPISI:
    /// {
    ///   "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
    ///   "preterite":  { "ich":"ging",  ... },
    ///   "perfect":    { "ich":"bin gegangen", ... },
    ///   "pastParticiple": "gegangen",
    ///   "auxiliaryVerb":  "sein"
    /// }
    /// </summary>
    public string? ConjugationData { get; set; }

    // ─── Ayrılabilir Fiiller ─────────────────────────────────────────────────

    /// <summary>
    /// Ayrılabilir fiil mi? (örn: "aufmachen" → "Ich mache die Tür auf.")
    /// NEDEN ÖNEMLİ: Ayrılabilir fiiller cümle sonunda ön ek alır — özel kart tasarımı gerekir.
    /// </summary>
    public bool IsSeparableVerb { get; set; } = false;

    /// <summary>
    /// Ayrılabilir ön ek: an, auf, ein, mit, vor, zu, vb.
    /// NULL ise IsSeparableVerb false demektir.
    /// </summary>
    public string? SeparablePrefix { get; set; }

    // ─── Telaffuz ve Medya ───────────────────────────────────────────────────

    /// <summary>IPA (Uluslararası Fonetik Alfabe) notasyonu — telaffuz rehberi</summary>
    public string? Pronunciation { get; set; }

    /// <summary>Seslendirme dosyasının URL'si</summary>
    public string? AudioUrl { get; set; }

    /// <summary>Görsel yardımcı URL'si (bellek kancası için)</summary>
    public string? ImageUrl { get; set; }

    // ─── Notlar ──────────────────────────────────────────────────────────────

    /// <summary>Öğretmen notları, kullanım ipuçları (serbest metin)</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Öğrencilerin sık yaptığı hatalar.
    /// ÖRNEK: "'der Hund' denir, 'die Hund' yanlış."
    /// </summary>
    public string? CommonMistakes { get; set; }

    /// <summary>Kayıt oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bu detayın bağlı olduğu kelime (N:1)</summary>
    public Word Word { get; set; } = null!;
}
