using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(WordLearnerDbContext db)
        : base(db) { }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) => _set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    // Replay tespitinde tüm family güvensiz sayılır — tek token değil, hepsi iptal edilir.
    public async Task RevokeFamilyAsync(string tokenFamily, CancellationToken ct = default)
    {
        var tokens = await _set.Where(t => t.TokenFamily == tokenFamily && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var tokens = await _set.Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
