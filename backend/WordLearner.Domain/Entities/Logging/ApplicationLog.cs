// ─────────────────────────────────────────────────────────────────────────────
// ApplicationLog.cs
//
// AMAÇ: Teknik log kaydı (hata/uyarı/bilgi) — Serilog'un MSSqlServer sink'i tarafından yazılır.
// NEDEN: BaseEntity'den TÜRETİLMEZ (ActivityLog.cs'teki gerekçeyle aynı — insert-only,
//        değişmez). Bu entity EF Core INSERT için değil, yalnızca A-07/B-08'in log
//        görüntüleme sorguları (SELECT) için tanımlıdır — satırları Serilog kendi SQL'iyle
//        yazar, `WordLearnerDbContext` üzerinden hiçbir yerde `Add`/`Update` çağrılmaz.
// BAĞIMLILIKLAR: Yok (Serilog sink kolonlarıyla bire bir eşleşir — bkz.
//                TECHNICAL_SPECIFICATIONS.md §9, ApplicationLogConfiguration).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Logging;

public class ApplicationLog
{
    public long Id { get; set; }

    // AMAÇ: Serilog seviyesi (Verbose|Debug|Information|Warning|Error|Fatal).
    public string Level { get; set; } = string.Empty;

    // AMAÇ: Şablonla doldurulmuş, okunabilir nihai mesaj.
    public string Message { get; set; } = string.Empty;

    // AMAÇ: Varsa fırlatılan exception'ın tam stack trace'i.
    public string? Exception { get; set; }

    // AMAÇ: Logu üreten sınıf (Serilog'un otomatik `SourceContext` özelliği).
    public string? SourceContext { get; set; }

    // AMAÇ: Logun üretildiği HTTP isteğinin yolu — RequestResponseLoggingMiddleware'in
    //       LogContext'e pushladığı özel alan (Serilog standardında yok).
    public string? RequestPath { get; set; }

    // AMAÇ: İsteği yapan kullanıcı (kimliği doğrulanmışsa) — aynı middleware pushlar.
    public int? UserId { get; set; }

    // AMAÇ: Serilog'un yapılandırılmış log özelliklerinin (structured properties) JSON serileştirmesi.
    public string? Properties { get; set; }

    // AMAÇ: Log satırının üretildiği an (UTC).
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
