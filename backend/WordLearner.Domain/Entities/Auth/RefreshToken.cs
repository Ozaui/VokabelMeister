namespace WordLearner.Domain.Entities.Auth;

// BaseEntity'den BİLİNÇLİ olarak türemez — soft delete yerine kendi geçersizleştirme deseni var
// (IsUsed/RevokedAt, Token Family Pattern); DATABASE_SCHEMA/Auth.md'deki RefreshTokens tablosuyla
// birebir eşleşir.
public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string TokenFamily { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
