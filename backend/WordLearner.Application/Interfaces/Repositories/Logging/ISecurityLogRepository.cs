using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories.Logging;

public interface ISecurityLogRepository
{
    Task AddAsync(SecurityLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
