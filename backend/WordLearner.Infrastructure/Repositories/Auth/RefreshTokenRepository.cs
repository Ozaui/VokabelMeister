using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Auth;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly WordLearnerDbContext _context;

    public RefreshTokenRepository(WordLearnerDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public async Task RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    public async Task RevokeFamilyAsync(string tokenFamily, CancellationToken cancellationToken = default)
    {
        var familyTokens = await _context.RefreshTokens
            .Where(t => t.TokenFamily == tokenFamily && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in familyTokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
