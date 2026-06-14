/// <summary>
/// Class.cs
///
/// AMAÇ:
///   Öğretmen/Instructor tarafından oluşturulan sanal sınıfları temsil eder.
///   Öğrenciler davet koduyla sınıfa katılır ve sınıfa atanan kategorileri çalışabilir.
///
/// NEDEN:
///   Kurumsal öğrenme senaryosu: Dil okulu öğretmeni öğrencilerine kelime listesi atar.
///   InviteCode ile QR kod veya link paylaşımı kolaylaşır.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - User (N:1 — sınıf sahibi)
///   - ClassMembership (1:N — üyeler)
///   - ClassCategory (M:N — atanan sistem kategorileri)
///   - ClassUserCategory (M:N — atanan kişisel kategoriler)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sanal sınıf entity'si.
///
/// AMAÇ: Öğretmen-öğrenci grubu oluşturmak ve ortak çalışma içeriği atamak.
/// NEDEN: Eğitim kurumları veya dil öğrenme grupları için kolektif öğrenme ortamı.
/// </summary>
public class Class : BaseEntity
{
    /// <summary>Sınıfı oluşturan kullanıcı ID'si (FK → Users) — Instructor veya Admin</summary>
    public int OwnerId { get; set; }

    /// <summary>Sınıf adı (max 100 karakter)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sınıf açıklaması (opsiyonel, max 500 karakter)</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Katılım daveti kodu (max 20 karakter, UNIQUE).
    /// ÖRNEK: "ABC123", "BERLIN24"
    /// NASIL KULLANILIR: Öğrenci bu kodu girerек sınıfa katılır.
    /// </summary>
    public string InviteCode { get; set; } = string.Empty;

    /// <summary>Sınıf aktif mi? Pasif sınıflar yeni üye kabul etmez.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Sınıf sahibi kullanıcı (N:1)</summary>
    public User Owner { get; set; } = null!;

    /// <summary>Sınıf üyelikleri (1:N)</summary>
    public ICollection<ClassMembership> ClassMemberships { get; set; } = new List<ClassMembership>();

    /// <summary>Sınıfa atanan sistem kategorileri (M:N ara tablo)</summary>
    public ICollection<ClassCategory> ClassCategories { get; set; } = new List<ClassCategory>();

    /// <summary>Sınıfa atanan kişisel kategoriler (M:N ara tablo)</summary>
    public ICollection<ClassUserCategory> ClassUserCategories { get; set; } = new List<ClassUserCategory>();
}
