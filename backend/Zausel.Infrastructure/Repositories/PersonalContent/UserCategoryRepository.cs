using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.PersonalContent;

public class UserCategoryRepository : IUserCategoryRepository
{
    private readonly ZauselDbContext _context;

    public UserCategoryRepository(ZauselDbContext context) => _context = context;

    public async Task<List<UserCategory>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.UserCategories.Where(c => c.UserId == userId).OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task<UserCategory?> GetByIdForUserAsync(int userCategoryId, int userId, CancellationToken cancellationToken = default) =>
        await _context.UserCategories.FirstOrDefaultAsync(c => c.Id == userCategoryId && c.UserId == userId, cancellationToken);

    public async Task<bool> AllExistForUserAsync(List<int> userCategoryIds, int userId, CancellationToken cancellationToken = default)
    {
        if (userCategoryIds.Count == 0)
            return true;

        var distinctIds = userCategoryIds.Distinct().ToList();
        var matchCount = await _context.UserCategories
            .Where(c => c.UserId == userId && distinctIds.Contains(c.Id))
            .CountAsync(cancellationToken);
        return matchCount == distinctIds.Count;
    }

    public async Task<Dictionary<int, int>> GetCardCountsAsync(List<int> userCategoryIds, CancellationToken cancellationToken = default)
    {
        if (userCategoryIds.Count == 0)
            return [];

        return await _context.UserCardUserCategories
            .Where(link => userCategoryIds.Contains(link.UserCategoryId))
            .GroupBy(link => link.UserCategoryId)
            .Select(g => new { UserCategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserCategoryId, g => g.Count, cancellationToken);
    }

    public async Task AddAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default)
    {
        userCategory.CreatedByUserId = userId;
        userCategory.UpdatedByUserId = userId;
        await _context.UserCategories.AddAsync(userCategory, cancellationToken);
    }

    public Task UpdateAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default)
    {
        userCategory.UpdatedByUserId = userId;
        _context.UserCategories.Update(userCategory);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default)
    {
        userCategory.IsDeleted = true;
        userCategory.DeletedAt = DateTime.UtcNow;
        userCategory.DeletedByUserId = userId;
        userCategory.UpdatedByUserId = userId;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
