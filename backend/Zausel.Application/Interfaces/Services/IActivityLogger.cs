namespace Zausel.Application.Interfaces.Services;

public interface IActivityLogger
{
    Task LogAsync(
        int? userId,
        string? actorRole,
        string action,
        string? entityType = null,
        int? entityId = null,
        object? oldValue = null,
        object? newValue = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
