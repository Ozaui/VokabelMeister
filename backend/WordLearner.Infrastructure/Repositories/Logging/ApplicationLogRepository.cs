using Microsoft.EntityFrameworkCore;
using WordLearner.Application.DTOs;
using WordLearner.Application.Interfaces.Repositories.Logging;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Logging;

public class ApplicationLogRepository : IApplicationLogRepository
{
    private readonly WordLearnerDbContext _context;

    public ApplicationLogRepository(WordLearnerDbContext context) => _context = context;

    public async Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level, DateTime? from, DateTime? to, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ApplicationLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(level)) query = query.Where(l => l.Level == level);
        if (from is not null) query = query.Where(l => l.TimeStamp >= from);
        if (to is not null) query = query.Where(l => l.TimeStamp <= to);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(l => l.Message.Contains(search));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationLog> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}
