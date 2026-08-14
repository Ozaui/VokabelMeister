using WordLearner.Application.Interfaces.Repositories.Logging;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Logging;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly WordLearnerDbContext _context;

    public ActivityLogRepository(WordLearnerDbContext context) => _context = context;

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default) =>
        await _context.ActivityLogs.AddAsync(log, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
