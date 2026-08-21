using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Srs;

public class UserCardProgressRepository : IUserCardProgressRepository
{
    private readonly ZauselDbContext _context;

    public UserCardProgressRepository(ZauselDbContext context) => _context = context;

    public async Task<List<ProgressSnapshot>> GetSnapshotsAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.UserCardProgress
            .Where(p => p.UserId == userId)
            .Select(p => new ProgressSnapshot(p.Mastery, p.NextReviewAt, p.IsSuspended, p.CurrentLevel))
            .ToListAsync(cancellationToken);
}
