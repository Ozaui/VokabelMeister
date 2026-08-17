using Zausel.Application.DTOs;
using Zausel.Domain.Entities.Logging;

namespace Zausel.Application.Interfaces.Repositories.Logging;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId, string? action, string? entityType, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
