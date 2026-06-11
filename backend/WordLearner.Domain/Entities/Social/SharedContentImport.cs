/// <summary>
/// SharedContentImport.cs
///
/// AMAÇ:
///   Bir kullanıcı paylaşım linki aracılığıyla içerik kopyaladığında oluşan kaydı saklar.
///   Aynı kullanıcı aynı içeriği yalnızca bir kez kopyalayabilir (UNIQUE kısıtı).
///
/// NEDEN:
///   ViewCount istatistiğinden farklı olarak gerçek "kopyalama" eylemini izler.
///   Kullanıcı tekrar aynı linke girdiğinde "Zaten kopyaladınız" uyarısı verilebilir.
///
/// BAĞIMLILIKLAR:
///   - SharedContent (N:1 — hangi paylaşım linkinden)
///   - User (N:1 — kopyalayan kullanıcı)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Paylaşım kopyalama kaydı entity'si.
///
/// AMAÇ: Bir paylaşım linkinden kimin ne zaman içerik kopyaladığını kayıt altına almak.
/// NEDEN BaseEntity'den miras almaz: Soft delete yoktur; kopyalama geri alınmaz.
/// </summary>
public class SharedContentImport
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Kopyalanan paylaşım linki ID'si (FK → SharedContents)</summary>
    public int SharedContentId { get; set; }

    /// <summary>Kopyalamayı yapan kullanıcı ID'si (FK → Users)</summary>
    public int ImportedByUserId { get; set; }

    /// <summary>Kopyalama tarihi (UTC)</summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Kopyalanan paylaşım linki (N:1)</summary>
    public SharedContent SharedContent { get; set; } = null!;

    /// <summary>Kopyalamayı yapan kullanıcı (N:1)</summary>
    public User ImportedByUser { get; set; } = null!;
}
