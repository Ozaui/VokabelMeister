using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<PagedResult<User>> GetPagedAsync(
        string? search,
        string? role,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    Task<(int TotalUsers, int ActiveUsers, int FrozenUsers)> GetStatisticsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DateTime>> GetRegistrationDatesAsync(DateTime fromUtc, CancellationToken ct = default);

    // Soft delete filtresini YOK SAYAR — grace period içindeki bir hesap login/register'da
    // görünmelidir (hesap kurtarma, tekrar kayıt engeli); IsDeleted/IsAnonymized kontrolü çağıranda yapılır.
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    // Soft delete filtresini YOK SAYAR — bkz. GetByEmailAsync.
    Task<User?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default);

    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
    Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default);

    // Anonimleştirilmiş bir hesabın e-postasıyla tekrar kayıt açılmasını engellemek için.
    Task<bool> OriginalEmailHashExistsAsync(string emailHash, CancellationToken ct = default);
}
