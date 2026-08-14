using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories.Logging;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
