// ─────────────────────────────────────────────────────────────────────────────
// ApplicationLogRepository.cs
//
// AMAÇ: IApplicationLogRepository'nin EF Core implementasyonu — yalnızca OKUMA.
// NEDEN: Bu tabloya Serilog'un MSSqlServer sink'i yazar (bkz. arayüzdeki NEDEN notu);
//        burada Add yoktur.
// BAĞIMLILIKLAR: EF Core, WordLearnerDbContext, ApplicationLog entity, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class ApplicationLogRepository : IApplicationLogRepository
{
    private readonly WordLearnerDbContext _db;

    public ApplicationLogRepository(WordLearnerDbContext db) => _db = db;

    public async Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level,
        DateTime? from,
        DateTime? to,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _db.ApplicationLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(a => a.Level == level);
        if (from is not null)
            query = query.Where(a => a.TimeStamp >= from);
        if (to is not null)
            query = query.Where(a => a.TimeStamp <= to);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Message.Contains(search));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ApplicationLog>(items, totalCount, page, pageSize);
    }
}
