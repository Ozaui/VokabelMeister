/// <summary>
/// ClassMembership.cs
///
/// AMAÇ:
///   Class (sınıf) ile User arasındaki M:N ilişkiyi yönetir.
///   Bir kullanıcının birden fazla sınıfa üye olabileceğini, bir sınıfın da birden fazla
///   üyesi olabileceğini modeller.
///
/// NEDEN:
///   Üyelik rolü (Student/Teacher) bu ara tabloda tutulur.
///   Aynı kişi bir sınıfta Teacher, başka bir sınıfta Student olabilir.
///
/// BAĞIMLILIKLAR:
///   - Class (N:1)
///   - User (N:1)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sınıf üyeliği entity'si (Class ↔ User M:N ara tablo).
///
/// AMAÇ: Hangi kullanıcının hangi sınıfa üye olduğunu ve rolünü saklamak.
/// NEDEN BaseEntity'den miras almaz: Soft delete yoktur; sınıf silinince CASCADE ile silinir.
/// </summary>
public class ClassMembership
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Sınıf ID'si (FK → Classes)</summary>
    public int ClassId { get; set; }

    /// <summary>Üye kullanıcı ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Sınıftaki rol: Student | Teacher
    /// Teacher: Sınıfa içerik ekleyebilir, üyeleri yönetebilir.
    /// Student: Sadece içeriği görüntüler ve çalışır.
    /// </summary>
    public string Role { get; set; } = "Student";

    /// <summary>Sınıfa katılma tarihi (UTC)</summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Üyelik aktif mi? Pasif üyeler sınıf içeriğine erişemez.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Üyenin ait olduğu sınıf (N:1)</summary>
    public Class Class { get; set; } = null!;

    /// <summary>Üye kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;
}
