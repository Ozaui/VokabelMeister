using Zausel.Application.DTOs;
using Zausel.Domain.Entities.Logging;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Interfaces.Repositories.Logging;

public interface ISecurityLogRepository
{
    Task AddAsync(SecurityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType, string? ipAddress, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
