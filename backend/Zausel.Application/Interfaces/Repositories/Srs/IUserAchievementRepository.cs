namespace Zausel.Application.Interfaces.Repositories.Srs;

public interface IUserAchievementRepository
{
    Task<bool> HasUnlockedAsync(int userId, int achievementId, CancellationToken cancellationToken = default);

    // UnlockedAt DB varsayılanıyla (GETUTCDATE()) dolar, çağıran SaveChangesAsync'i AYRI çağırır
    // (AchievementService birden çok rozeti TEK SaveChanges'te toplu yazabilsin diye).
    Task AddAsync(int userId, int achievementId, CancellationToken cancellationToken = default);

    Task<List<AchievementUnlockItem>> GetUnlockedForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
