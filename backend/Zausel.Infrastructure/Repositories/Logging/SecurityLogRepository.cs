using Microsoft.EntityFrameworkCore;
using Zausel.Application.DTOs;
using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Domain.Entities.Logging;
using Zausel.Domain.Enums.Logging;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Logging;

public class SecurityLogRepository : ISecurityLogRepository
{
    private readonly ZauselDbContext _context;

    public SecurityLogRepository(ZauselDbContext context) => _context = context;

    public async Task AddAsync(SecurityLog log, CancellationToken cancellationToken = default) =>
        await _context.SecurityLogs.AddAsync(log, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType, string? ipAddress, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.SecurityLogs.AsQueryable();

        if (eventType is not null) query = query.Where(l => l.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(ipAddress)) query = query.Where(l => l.IpAddress == ipAddress);
        if (from is not null) query = query.Where(l => l.CreatedAt >= from);
        if (to is not null) query = query.Where(l => l.CreatedAt <= to);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SecurityLog> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}
