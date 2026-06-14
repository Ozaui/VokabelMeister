/// <summary>
/// UserCategory.cs
///
/// AMAÇ:
///   Kullanıcının kendisi için oluşturduğu kişisel kategorileri temsil eder.
///   Sistem kategorilerinden (Category) bağımsız olarak çalışır.
///
/// NEDEN:
///   Kullanıcılar kendi öğrenme planlarına göre özel gruplar oluşturabilir.
///   Örn: "Sınav Hazırlık", "Çalışma Arkadaşlarıyla Paylaşılan" gibi gruplar.
///   Sistem kategorileri admin onaylıdır; kişisel kategoriler tamamen özgürdür.
///
/// GÜVENLİK NOTU:
///   Repository sorgularında her zaman UserId filtresi ZORUNLUDUR.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - User (N:1 — kategori sahibi)
///   - UserCardUserCategory (M:N ara tablo — hangi kartlar bu kategoride)
///   - ClassUserCategory (M:N ara tablo — hangi sınıflar bu kategoriye atanmış)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kişisel kategori entity'si.
///
/// AMAÇ: Kullanıcının kendi flash kartlarını gruplandırması için özel kategoriler.
/// NEDEN: Sistem kategorilerine ek olarak kişiselleştirme imkânı sağlamak.
/// </summary>
public class UserCategory : BaseEntity
{
    /// <summary>Kategori sahibi kullanıcının ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>Kategori adı (max 100 karakter)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Açıklama (opsiyonel, max 500 karakter)</summary>
    public string? Description { get; set; }

    /// <summary>Hex renk kodu (örn: "#4ECDC4") — UI'da görsel ayrım için</summary>
    public string? Color { get; set; }

    /// <summary>İkon tanımlayıcısı</summary>
    public string? Icon { get; set; }

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Kategori sahibi kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>Bu kategorideki kişisel kartlar (M:N ara tablo)</summary>
    public ICollection<UserCardUserCategory> UserCardUserCategories { get; set; } = new List<UserCardUserCategory>();

    /// <summary>Bu kategoriye atanan sınıflar (M:N ara tablo)</summary>
    public ICollection<ClassUserCategory> ClassUserCategories { get; set; } = new List<ClassUserCategory>();
}
