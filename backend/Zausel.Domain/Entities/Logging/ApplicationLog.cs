namespace Zausel.Domain.Entities.Logging;

// BaseEntity'den BİLİNÇLİ olarak türemez — Serilog MSSqlServer sink'in kendi yazdığı teknik log,
// insert-only; DATABASE_SCHEMA/Loglama.md'deki ApplicationLogs tablosuyla birebir eşleşir. FK yok
// (sink User tablosuna join/kontrol yapmaz, ham UserId int'i yazar).
public class ApplicationLog
{
    public long Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? SourceContext { get; set; }
    public string? RequestPath { get; set; }
    public int? UserId { get; set; }
    public string? Properties { get; set; }
    public DateTime TimeStamp { get; set; }
}
