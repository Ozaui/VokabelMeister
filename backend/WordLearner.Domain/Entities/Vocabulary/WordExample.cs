/// <summary>
/// WordExample.cs
///
/// AMAÇ:
///   Bir sistem kelimesine ait seviyeli örnek cümleleri saklar.
///   Her cümle hem Almanca hem Türkçe (opsiyonel İngilizce) çeviriyle gelir.
///
/// NEDEN:
///   Bağlam içinde öğrenme (contextual learning) kelime kalıcılığını artırır.
///   Farklı seviyeler (A1–C2) ve türler (Normal, Idiom, Formal, Colloquial)
///   sayesinde kullanıcı seviyesine uygun cümle seçilebilir.
///
/// BAĞIMLILIKLAR:
///   - Word (N:1 — hangi kelimeye ait)
///   - User (N:1, CreatedBy — kim ekledi, opsiyonel)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sistem kelimesi örnek cümle entity'si.
///
/// AMAÇ: Kelimeyi bağlam içinde gösteren seviyeli ve türlü cümleleri saklamak.
/// NEDEN BaseEntity'den miras almaz: Soft delete gerekmez — Word silinince CASCADE ile silinir.
/// </summary>
public class WordExample
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Ait olduğu kelimenin ID'si (FK → Words)</summary>
    public int WordId { get; set; }

    /// <summary>
    /// Almanca örnek cümle.
    /// ÖRNEK: "Der Hund läuft schnell."
    /// </summary>
    public string SentenceDE { get; set; } = string.Empty;

    /// <summary>
    /// Türkçe çeviri.
    /// ÖRNEK: "Köpek hızlı koşar."
    /// </summary>
    public string SentenceTR { get; set; } = string.Empty;

    /// <summary>
    /// Bu cümlenin gösterileceği minimum CEFR seviyesi: A1 | A2 | B1 | B2 | C1 | C2
    /// NASIL KULLANILIR: Kullanıcının CurrentLevel'ına uygun cümleler filtrelenir.
    ///                  A1 kullanıcısına C2 cümlesi gösterilmez.
    /// </summary>
    public string Level { get; set; } = "A1";

    /// <summary>
    /// Cümle türü: Normal | Idiom | Formal | Colloquial
    /// Normal     : Standart günlük kullanım
    /// Idiom      : Deyimsel kullanım (örn: "Das ist mir Wurst." = "Umurumda değil.")
    /// Formal     : Resmi dil (iş e-postası, gazete)
    /// Colloquial : Günlük konuşma dili (kısaltmalar, slang)
    /// </summary>
    public string ExampleType { get; set; } = "Normal";

    /// <summary>Gösterim sırası — küçük değer önce gösterilir.</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Aktif mi? Pasif cümleler kullanıcılara gösterilmez.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Cümleyi ekleyen kullanıcı ID'si (FK → Users, NULL = sistem)</summary>
    public int? CreatedBy { get; set; }

    /// <summary>Oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı olduğu kelime (N:1)</summary>
    public Word Word { get; set; } = null!;

    /// <summary>Cümleyi ekleyen kullanıcı (N:1, opsiyonel)</summary>
    public User? Creator { get; set; }
}
