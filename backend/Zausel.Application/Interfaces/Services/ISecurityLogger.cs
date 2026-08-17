using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Interfaces.Services;

public interface ISecurityLogger
{
    Task LogAsync(
        LogEventType eventType,
        int? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? detail = null,
        CancellationToken cancellationToken = default);
}
