namespace WordLearner.Application.Interfaces.Services;

public interface IActivityLogger
{
    // oldValue/newValue verilirse JSON'a serileştirilip OldValue/NewValue kolonlarına yazılır;
    // yalnızca oluşturma/silme gibi tek durumlu eylemlerde biri (genelde newValue) yeterlidir.
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
        CancellationToken ct = default
    );
}
