using WordLearner.Application.DTOs;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories.Logging;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId, string? action, string? entityType, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
