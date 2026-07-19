// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogRepository.cs
//
// AMAÇ: IActivityLogRepository'nin EF Core implementasyonu.
// NEDEN: Repository<T>'yi miras ALMAZ (ActivityLog BaseEntity'den türemiyor — bkz.
//        IActivityLogRepository.cs'teki NEDEN notu); doğrudan WordLearnerDbContext
//        enjekte edilir.
// BAĞIMLILIKLAR: EF Core, WordLearnerDbContext, ActivityLog entity, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly WordLearnerDbContext _db;

    public ActivityLogRepository(WordLearnerDbContext db) => _db = db;

    public async Task AddAsync(ActivityLog log, CancellationToken ct = default)
    {
        await _db.ActivityLogs.AddAsync(log, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId,
        string? action,
        string? entityType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _db.ActivityLogs.AsQueryable();

        if (userId is not null)
            query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (from is not null)
            query = query.Where(a => a.CreatedAt >= from);
        if (to is not null)
            query = query.Where(a => a.CreatedAt <= to);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ActivityLog>(items, totalCount, page, pageSize);
    }
}
