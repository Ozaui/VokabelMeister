using WordLearner.Application.DTOs;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Interfaces.Repositories.Logging;

public interface ISecurityLogRepository
{
    Task AddAsync(SecurityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType, string? ipAddress, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
