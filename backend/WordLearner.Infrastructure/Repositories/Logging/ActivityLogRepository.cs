using Microsoft.EntityFrameworkCore;
using WordLearner.Application.DTOs;
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

    public async Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId, string? action, string? entityType, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (userId is not null) query = query.Where(l => l.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(l => l.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(l => l.EntityType == entityType);
        if (from is not null) query = query.Where(l => l.CreatedAt >= from);
        if (to is not null) query = query.Where(l => l.CreatedAt <= to);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ActivityLog> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}
