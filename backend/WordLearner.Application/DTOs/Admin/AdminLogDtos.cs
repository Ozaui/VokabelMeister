namespace WordLearner.Application.DTOs.Admin;

public record ActivityLogDto(
    long Id,
    int? UserId,
    string? ActorRole,
    string Action,
    string? EntityType,
    int? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);

public record ApplicationLogDto(
    long Id,
    string Level,
    string Message,
    string? Exception,
    string? SourceContext,
    string? RequestPath,
    int? UserId,
    DateTime TimeStamp
);

// Detail, admin isteğinin dilinde ÇÖZÜLMÜŞ metni taşır (LogMessages.Resolve).
public record SecurityLogDto(
    long Id,
    string EventType,
    int? UserId,
    string? IpAddress,
    string? UserAgent,
    string? Detail,
    DateTime CreatedAt
);
