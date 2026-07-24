using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

// Add yok — bu tabloya satırları Serilog'un MSSqlServer sink'i yazar, Application katmanı yalnızca okur.
public interface IApplicationLogRepository
{
    Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level,
        DateTime? from,
        DateTime? to,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
