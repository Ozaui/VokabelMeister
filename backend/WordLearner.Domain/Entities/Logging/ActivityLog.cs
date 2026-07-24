using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Domain.Entities.Logging;

// "Kim ne yaptı" audit kaydı. BaseEntity'den TÜREMEZ — log tabloları insert-only ve
// değişmez, soft delete/UpdatedAt gibi alanlar burada anlamsız.
public class ActivityLog
{
    // BIGINT — yüksek hacimli insert-only tabloda int sınırı yetersiz kalabilir.
    public long Id { get; set; }

    // Anonim eylemlerde (ör. login öncesi) NULL.
    public int? UserId { get; set; }

    // Yazıldığı andaki rol — kullanıcının rolü sonradan değişse bile donmuş kalır.
    public string? ActorRole { get; set; }

    // Sabit eylem kodu (LOGIN, CREATE_WORD, ...) — enum değil, her yeni feature kendi string'ini ekler.
    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}
