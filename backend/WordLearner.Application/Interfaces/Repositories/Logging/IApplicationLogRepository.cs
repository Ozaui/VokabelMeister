using WordLearner.Application.DTOs;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories.Logging;

public interface IApplicationLogRepository
{
    Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level, DateTime? from, DateTime? to, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default);
}
