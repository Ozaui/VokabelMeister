namespace Zausel.Domain.Entities.Logging;

// BaseEntity'den BİLİNÇLİ olarak türemez — audit log insert-only, soft delete/UpdatedAt anlamsız
// (bir log satırı ne silinir ne güncellenir); DATABASE_SCHEMA/Loglama.md'deki ActivityLogs
// tablosuyla birebir eşleşir.
public class ActivityLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string? ActorRole { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
