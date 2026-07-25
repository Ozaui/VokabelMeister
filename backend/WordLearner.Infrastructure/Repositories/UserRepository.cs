using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(WordLearnerDbContext db)
        : base(db) { }

    public async Task<PagedResult<User>> GetPagedAsync(
        string? search,
        string? role,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _set.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Email.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search)
            );
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<User>(items, totalCount, page, pageSize);
    }

    public async Task<(int TotalUsers, int ActiveUsers, int FrozenUsers)> GetStatisticsAsync(
        CancellationToken ct = default
    )
    {
        var total = await _set.CountAsync(ct);
        var active = await _set.CountAsync(u => u.IsActive, ct);
        return (total, active, total - active);
    }

    public async Task<IReadOnlyList<DateTime>> GetRegistrationDatesAsync(
        DateTime fromUtc,
        CancellationToken ct = default
    ) => await _set.Where(u => u.CreatedAt >= fromUtc).Select(u => u.CreatedAt).ToListAsync(ct);

    // IgnoreQueryFilters — grace period içindeki (soft-delete'li) hesap login/register'da
    // görünmezse hesap kurtarma ve "e-posta zaten kullanımda" kontrolü çalışamaz.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.AppleId == appleId, ct);

    public Task<bool> OriginalEmailHashExistsAsync(
        string emailHash,
        CancellationToken ct = default
    ) => _set.IgnoreQueryFilters().AnyAsync(u => u.OriginalEmailHash == emailHash, ct);

    public async Task<IReadOnlyList<User>> GetPendingAnonymizationAsync(
        DateTime utcNow,
        CancellationToken ct = default
    ) =>
        await _set.IgnoreQueryFilters()
            .Where(u =>
                u.IsDeleted
                && !u.IsAnonymized
                && u.ScheduledDeletionAt != null
                && u.ScheduledDeletionAt <= utcNow
            )
            .OrderBy(u => u.Id)
            .ToListAsync(ct);
}
