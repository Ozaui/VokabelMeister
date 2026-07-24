namespace WordLearner.Domain.Entities.Auth;

// Token Family Pattern: her refresh'te eski token IsUsed=true olur; aynı family'den
// ikinci kullanım replay sayılır ve tüm family iptal edilir (SECURITY.md §1).
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;

    // Aynı login oturumundan türeyen tüm token'ları gruplar — replay tespiti bunun üzerinden yapılır.
    public string TokenFamily { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}
