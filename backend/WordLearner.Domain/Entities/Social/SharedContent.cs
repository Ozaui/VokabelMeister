/// <summary>
/// SharedContent.cs
///
/// AMAÇ:
///   Kullanıcıların içeriklerini (UserCard, UserCategory, Class) paylaşım linki
///   aracılığıyla başkalarıyla paylaşmasını sağlar.
///
/// NEDEN:
///   Giriş yapmadan önizleme + tek tıkla kopyalama, virallik için önemli.
///   ShareToken (UUID) URL'de kullanılır — tahmin edilemez, güvenli.
///   ExpiresAt ile geçici linkler oluşturulabilir.
///
/// BAĞIMLILIKLAR:
///   - User (N:1 — içerik sahibi)
///   - SharedContentImport (1:N — bu linki kullanarak kopyalayan kullanıcılar)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Paylaşım linki entity'si.
///
/// AMAÇ: Kullanıcı içeriklerini link aracılığıyla paylaşmak için UUID token yönetimi.
/// NEDEN BaseEntity'den miras almaz: Soft delete yoktur; IsActive bayrağı yeterlidir.
/// </summary>
public class SharedContent
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>İçerik sahibi kullanıcı ID'si (FK → Users)</summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// UUID paylaşım tokeni (36 karakter, UNIQUE).
    /// ÖRNEK: "550e8400-e29b-41d4-a716-446655440000"
    /// NASIL KULLANILIR: https://app.wordlearner.com/share/{ShareToken}
    /// </summary>
    public string ShareToken { get; set; } = string.Empty;

    /// <summary>
    /// Paylaşılan içerik türü: UserCard | UserCategory | Class
    /// NEDEN: İçerik türüne göre farklı tablo sorgulanır (polymorphic ilişki).
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Paylaşılan içeriğin ID'si (ContentType tablosundaki PK)</summary>
    public int ContentId { get; set; }

    /// <summary>Link aktif mi? False yapılırsa link çalışmaz ama kaydı silinmez.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Linkin geçerlilik bitiş tarihi (UTC).
    /// NULL = sonsuz süre geçerli.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Linkin kaç kez görüntülendiği sayacı (istatistik için)</summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>Link oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>İçerik sahibi kullanıcı (N:1)</summary>
    public User Owner { get; set; } = null!;

    /// <summary>Bu linki kullanarak içerik kopyalayanlar (1:N)</summary>
    public ICollection<SharedContentImport> SharedContentImports { get; set; } = new List<SharedContentImport>();
}
