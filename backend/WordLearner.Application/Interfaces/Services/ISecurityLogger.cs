using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Interfaces.Services;

public interface ISecurityLogger
{
    // email verilirse SHA-256(email) hash'i EmailHash'e yazılır (PII kuralı: ham e-posta
    // asla saklanmaz) — kayıtlı olmayan bir e-postayla login denemesinde UserId null olsa bile EmailHash dolar.
    Task LogAsync(
        LogEventType eventType,
        int? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? detail = null,
        CancellationToken ct = default
    );
}
