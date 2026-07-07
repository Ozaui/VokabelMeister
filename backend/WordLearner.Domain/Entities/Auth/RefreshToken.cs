// ─────────────────────────────────────────────────────────────────────────────
// RefreshToken.cs
//
// AMAÇ: Kullanıcının access token'ı yenilemek için kullandığı uzun ömürlü token kaydı.
// NEDEN: Token Family Pattern uygular — her refresh'te eski token tek kullanımlık
//        hâle gelir (IsUsed); aynı family'den ikinci kullanım replay saldırısı
//        sayılır ve tüm family iptal edilir (bkz. REFERENCE/SECURITY.md §1).
// BAĞIMLILIKLAR: BaseEntity, User (N:1 ilişki).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    // AMAÇ: Token'ın ait olduğu kullanıcı.
    public int UserId { get; set; }

    // AMAÇ: Navigation property — EF Core Include() ile kullanıcı bilgisine erişim sağlar.
    public User User { get; set; } = null!;

    // AMAÇ: Refresh token'ın SHA-256 hash'i. Plaintext asla saklanmaz.
    public string TokenHash { get; set; } = string.Empty;

    // AMAÇ: Aynı login oturumundan türeyen tüm refresh token'ları gruplayan GUID.
    // NEDEN: Replay tespiti bu alan üzerinden yapılır — kullanılmış bir token
    //        tekrar geldiğinde aynı family'deki TÜM token'lar iptal edilir.
    public string TokenFamily { get; set; } = string.Empty;

    // AMAÇ: Token'ın geçerlilik süresinin dolacağı an (UTC).
    public DateTime ExpiresAt { get; set; }

    // AMAÇ: Bu token'ın bir refresh işleminde zaten kullanılıp kullanılmadığı.
    // NEDEN: Refresh token tek kullanımlıktır; ikinci kullanım replay sayılır.
    public bool IsUsed { get; set; }

    // AMAÇ: Token'ın elle iptal edildiği an (logout veya replay tespiti sonucu).
    public DateTime? RevokedAt { get; set; }

    // AMAÇ: Token'ın üretildiği cihazın bilgisi (user-agent vb.) — audit amaçlı.
    public string? DeviceInfo { get; set; }

    // AMAÇ: Token'ın üretildiği IP adresi — audit amaçlı.
    public string? IpAddress { get; set; }
}
