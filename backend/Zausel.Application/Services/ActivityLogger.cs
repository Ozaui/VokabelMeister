using System.Text.Json;
using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Logging;

namespace Zausel.Application.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogger(IActivityLogRepository activityLogRepository) => _activityLogRepository = activityLogRepository;

    public async Task LogAsync(
        int? userId,
        string? actorRole,
        string action,
        string? entityType = null,
        int? entityId = null,
        object? oldValue = null,
        object? newValue = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            ActorRole = actorRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _activityLogRepository.AddAsync(log, cancellationToken);
        await _activityLogRepository.SaveChangesAsync(cancellationToken);
    }
}
