/// <summary>
/// Word.cs
///
/// AMAÇ:
///   Admin veya Instructor tarafından sisteme eklenen Almanca kelimeleri temsil eder.
///   Tüm kullanıcıların öğrenebileceği ortak kelime havuzunu oluşturur.
///
/// NEDEN:
///   Sistem kelimeleri (Words) ile kullanıcı kartları (UserCards) ayrı tutulur.
///   Böylece admin onaylı, kaliteli içerik kişisel notlardan bağımsız yönetilir.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - WordDetail (1:1 — Almanca gramer bilgisi: cinsiyet, artikeller, çekimler)
///   - WordExample (1:N — seviyeli örnek cümleler)
///   - WordCategory (M:N ara tablo — kelime ↔ kategori)
///   - UserProgress (1:N — kullanıcıların bu kelimeyi öğrenme ilerlemesi)
///   - LearningHistory (1:N — bu kelimeyle yapılan tüm girişimler)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sistem kelimesi entity'si.
///
/// AMAÇ: Admin onaylı Almanca-Türkçe kelime çiftlerini saklamak.
/// NEDEN: Tek merkezi kelime havuzu, tüm kullanıcıların aynı kaliteli içeriğe erişmesini sağlar.
/// </summary>
public class Word : BaseEntity
{
    /// <summary>
    /// Almanca kelime (max 255 karakter).
    /// ÖRNEK: "der Hund", "gehen", "schön"
    /// </summary>
    public string GermanWord { get; set; } = string.Empty;

    /// <summary>
    /// Türkçe çeviri (max 500 karakter — birden fazla anlam olabilir).
    /// ÖRNEK: "köpek", "gitmek", "güzel"
    /// </summary>
    public string TurkishTranslation { get; set; } = string.Empty;

    /// <summary>
    /// Kelime türü: Noun | Verb | Adjective | Adverb | Conjunction | Preposition | Pronoun | Other
    /// NEDEN: Kelime türüne göre farklı quiz soruları üretilir (artikel quiz sadece Noun için).
    /// </summary>
    public string PartOfSpeech { get; set; } = string.Empty;

    /// <summary>
    /// CEFR zorluk seviyesi: A1 | A2 | B1 | B2 | C1 | C2
    /// NASIL KULLANILIR: Kullanıcı seviyesine uygun kelimeler filtrelenir.
    /// </summary>
    public string DifficultyLevel { get; set; } = "A1";

    /// <summary>
    /// Kelimenin tanımı veya açıklaması (opsiyonel, serbest metin)
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>Kelime aktif mi? Pasif kelimeler kullanıcılara gösterilmez.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Kelimeyi onaylayan yönetici/eğitmen kullanıcı ID'si (FK → Users)</summary>
    public int? ApprovedBy { get; set; }

    /// <summary>Onay tarihi</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Kaydı oluşturan kullanıcı ID'si (FK → Users)</summary>
    public int? CreatedBy { get; set; }

    /// <summary>Son güncelleyen kullanıcı ID'si (FK → Users)</summary>
    public int? UpdatedBy { get; set; }

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Almanca gramer detayları (1:1) — cinsiyet, artikeller, çekimler, telaffuz</summary>
    public WordDetail? WordDetail { get; set; }

    /// <summary>Seviyeli örnek cümleler (1:N)</summary>
    public ICollection<WordExample> WordExamples { get; set; } = new List<WordExample>();

    /// <summary>Kategoriler (M:N ara tablo)</summary>
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();

    /// <summary>Kullanıcıların bu kelimeyi öğrenme durumları (1:N)</summary>
    public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();

    /// <summary>Bu kelimeyle yapılan tüm öğrenme girişimleri (1:N)</summary>
    public ICollection<LearningHistory> LearningHistories { get; set; } = new List<LearningHistory>();

    /// <summary>Onaylayan kullanıcı navigation (N:1)</summary>
    public User? Approver { get; set; }

    /// <summary>Oluşturan kullanıcı navigation (N:1)</summary>
    public User? Creator { get; set; }
}
