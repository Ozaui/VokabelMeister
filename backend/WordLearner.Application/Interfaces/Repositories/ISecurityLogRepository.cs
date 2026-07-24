using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ISecurityLogRepository
{
    Task AddAsync(SecurityLog log, CancellationToken ct = default);

    Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType,
        string? ipAddress,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
