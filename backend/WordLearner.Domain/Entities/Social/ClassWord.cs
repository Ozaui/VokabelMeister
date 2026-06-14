/// <summary>
/// ClassWord.cs
///
/// AMAÇ:
///   Instructor'ın kendi sınıfı için eklediği sınıfa özel kelimeleri temsil eder.
///   Bu kelimeler YALNIZCA o sınıfın üyelerine görünür; sistem Words tablosuna eklenmez.
///
/// NEDEN:
///   Instructor, tüm sisteme kelime ekleyemez — sadece kendi sınıfının öğrencileri için kelime yükler.
///   Sistem kelimeleri (Words) Admin'in yetkisindedir; bu tablo Instructor'a kontrollü içerik ekleme hakkı verir.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - Class (N:1 — ait olduğu sınıf)
///   - User (N:1 — ekleyen Instructor)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sınıfa özel kelime entity'si.
///
/// AMAÇ: Instructor'ın kendi sınıfına özel Almanca-Türkçe kelime eklemesini sağlamak.
/// NEDEN: Sistem kelimeleri Admin'e aittir; Instructor yalnızca kendi sınıf üyeleri için içerik üretebilir.
/// </summary>
public class ClassWord : BaseEntity
{
    /// <summary>Kelimenin ait olduğu sınıf ID'si (FK → Classes)</summary>
    public int ClassId { get; set; }

    /// <summary>Kelimeyi ekleyen Instructor'ın User ID'si (FK → Users)</summary>
    public int CreatedBy { get; set; }

    // ─── Kelime İçeriği ──────────────────────────────────────────────────────

    /// <summary>Almanca kelime (örn: "der Hund", "gehen", "schön")</summary>
    public string GermanWord { get; set; } = string.Empty;

    /// <summary>Türkçe çeviri</summary>
    public string TurkishTranslation { get; set; } = string.Empty;

    /// <summary>
    /// Kelime türü (opsiyonel).
    /// DEĞERLER: Noun | Verb | Adjective | Adverb | Conjunction | Preposition | Pronoun | Other
    /// </summary>
    public string? PartOfSpeech { get; set; }

    /// <summary>Öğretmenin eklediği ek not veya açıklama (opsiyonel)</summary>
    public string? Notes { get; set; }

    // ─── Almanca Gramer (İsimler için opsiyonel) ─────────────────────────────

    /// <summary>
    /// Cinsiyet (yalnızca isimler için).
    /// DEĞERLER: Masculine | Feminine | Neuter
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>Belirli artikel nominatif hâl (der / die / das)</summary>
    public string? ArticleDefiniteNom { get; set; }

    /// <summary>Çoğul formu (örn: Hunde, Männer)</summary>
    public string? PluralForm { get; set; }

    /// <summary>Kelime aktif mi? Pasif kelimeler sınıf üyelerine gösterilmez.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Kelimenin ait olduğu sınıf (N:1)</summary>
    public Class Class { get; set; } = null!;

    /// <summary>Kelimeyi ekleyen Instructor kullanıcısı (N:1)</summary>
    public User CreatedByUser { get; set; } = null!;
}
