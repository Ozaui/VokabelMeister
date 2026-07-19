// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogger.cs
//
// AMAÇ: IActivityLogger'ın implementasyonu.
// NEDEN: OldValue/NewValue serileştirmesini (System.Text.Json) ve ActivityLog
//        entity'sinin kurulmasını tek yerde toplar; Handler'lar JSON'la uğraşmaz.
// BAĞIMLILIKLAR: IActivityLogRepository.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IActivityLogRepository _repository;

    public ActivityLogger(IActivityLogRepository repository) => _repository = repository;

    public Task LogAsync(
        int? userId,
        string? actorRole,
        string action,
        string? entityType = null,
        int? entityId = null,
        object? oldValue = null,
        object? newValue = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default
    )
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
            UserAgent = userAgent,
        };

        return _repository.AddAsync(log, ct);
    }
}
