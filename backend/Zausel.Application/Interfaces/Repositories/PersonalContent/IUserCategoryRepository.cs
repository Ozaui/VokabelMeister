using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Interfaces.Repositories.PersonalContent;

public interface IUserCategoryRepository
{
    Task<List<UserCategory>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    // Sahiplik filtresi burada gömülü — başkasının kategorisi null döner, Handler'da EntityNotFoundException'a çevrilir.
    Task<UserCategory?> GetByIdForUserAsync(int userCategoryId, int userId, CancellationToken cancellationToken = default);

    // UserCardCommand'ların (A-10) categoryIds[] gibi userCategoryIds[] alanı için — verilen kimliklerin
    // TAMAMININ bu kullanıcıya ait olup olmadığını tek sorguda doğrular (başkasının kategorisine bağ
    // kurulmasını engeller).
    Task<bool> AllExistForUserAsync(List<int> userCategoryIds, int userId, CancellationToken cancellationToken = default);

    // GetUserCategoriesQuery (A-10) için — UserCategoryId → o kategoriye bağlı (UserCardUserCategories
    // üzerinden) kart sayısı.
    Task<Dictionary<int, int>> GetCardCountsAsync(List<int> userCategoryIds, CancellationToken cancellationToken = default);

    Task AddAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(UserCategory userCategory, int userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
