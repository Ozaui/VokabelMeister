using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

// IRepository<T>'yi MİRAS ALMAZ — ActivityLog BaseEntity'den türemiyor (insert-only log tablosu).
public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken ct = default);

    Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId,
        string? action,
        string? entityType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
