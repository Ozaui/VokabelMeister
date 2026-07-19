// ─────────────────────────────────────────────────────────────────────────────
// SecurityLogRepository.cs
//
// AMAÇ: ISecurityLogRepository'nin EF Core implementasyonu.
// NEDEN: ActivityLogRepository ile aynı gerekçeyle Repository<T>'yi miras almaz.
// BAĞIMLILIKLAR: EF Core, WordLearnerDbContext, SecurityLog entity, LogEventType enum, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class SecurityLogRepository : ISecurityLogRepository
{
    private readonly WordLearnerDbContext _db;

    public SecurityLogRepository(WordLearnerDbContext db) => _db = db;

    public async Task AddAsync(SecurityLog log, CancellationToken ct = default)
    {
        await _db.SecurityLogs.AddAsync(log, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType,
        string? ipAddress,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _db.SecurityLogs.AsQueryable();

        if (eventType is not null)
            query = query.Where(s => s.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(ipAddress))
            query = query.Where(s => s.IpAddress == ipAddress);
        if (from is not null)
            query = query.Where(s => s.CreatedAt >= from);
        if (to is not null)
            query = query.Where(s => s.CreatedAt <= to);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<SecurityLog>(items, totalCount, page, pageSize);
    }
}
