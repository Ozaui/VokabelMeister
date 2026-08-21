using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Domain.Entities.Srs;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Srs;

public class UserAchievementRepository : IUserAchievementRepository
{
    private readonly ZauselDbContext _context;

    public UserAchievementRepository(ZauselDbContext context) => _context = context;

    public async Task<bool> HasUnlockedAsync(int userId, int achievementId, CancellationToken cancellationToken = default) =>
        await _context.UserAchievements.AnyAsync(a => a.UserId == userId && a.AchievementId == achievementId, cancellationToken);

    public Task AddAsync(int userId, int achievementId, CancellationToken cancellationToken = default)
    {
        _context.UserAchievements.Add(new UserAchievement { UserId = userId, AchievementId = achievementId });
        return Task.CompletedTask;
    }

    public async Task<List<AchievementUnlockItem>> GetUnlockedForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Join(_context.Achievements, ua => ua.AchievementId, a => a.Id, (ua, a) =>
                new AchievementUnlockItem(a.Id, a.Icon, a.RewardXP, a.Rarity.ToString(), ua.UnlockedAt))
            .OrderByDescending(item => item.UnlockedAt)
            .ToListAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
