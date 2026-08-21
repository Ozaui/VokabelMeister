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
