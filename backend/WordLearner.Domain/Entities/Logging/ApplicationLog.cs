namespace WordLearner.Domain.Entities.Logging;

// Teknik log kaydı — Serilog'un MSSqlServer sink'i yazar; bu entity yalnızca SELECT
// içindir, WordLearnerDbContext üzerinden hiç Add/Update çağrılmaz.
public class ApplicationLog
{
    public long Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? SourceContext { get; set; }

    // RequestResponseLoggingMiddleware'in LogContext'e pushladığı özel alan.
    public string? RequestPath { get; set; }
    public int? UserId { get; set; }
    public string? Properties { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
