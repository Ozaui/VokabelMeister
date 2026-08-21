using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Interfaces.Repositories.PersonalContent;

public interface IUserCategoryRepository
{
    Task<List<UserCategory>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    // Sahiplik filtresi burada gömülü — başkasının kategorisi null döner, Handler'da EntityNotFoundException'a çevrilir.
    Task<UserCategory?> GetByIdForUserAsync(int userCategoryId, int userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
