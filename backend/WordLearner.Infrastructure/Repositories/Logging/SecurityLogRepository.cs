using WordLearner.Application.Interfaces.Repositories.Logging;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Logging;

public class SecurityLogRepository : ISecurityLogRepository
{
    private readonly WordLearnerDbContext _context;

    public SecurityLogRepository(WordLearnerDbContext context) => _context = context;

    public async Task AddAsync(SecurityLog log, CancellationToken cancellationToken = default) =>
        await _context.SecurityLogs.AddAsync(log, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
