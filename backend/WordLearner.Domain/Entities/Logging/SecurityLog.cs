using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Domain.Entities.Logging;

// BaseEntity'den BİLİNÇLİ olarak türemez — güvenlik olayı insert-only, soft delete/UpdatedAt
// anlamsız; DATABASE_SCHEMA/Loglama.md'deki SecurityLogs tablosuyla birebir eşleşir.
public class SecurityLog
{
    public long Id { get; set; }
    public LogEventType EventType { get; set; }
    public int? UserId { get; set; }
    public string? EmailHash { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; }
}
