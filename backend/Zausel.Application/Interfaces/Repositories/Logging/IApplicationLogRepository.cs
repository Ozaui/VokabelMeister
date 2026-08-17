using Zausel.Application.DTOs;
using Zausel.Domain.Entities.Logging;

namespace Zausel.Application.Interfaces.Repositories.Logging;

public interface IApplicationLogRepository
{
    Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level, DateTime? from, DateTime? to, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
