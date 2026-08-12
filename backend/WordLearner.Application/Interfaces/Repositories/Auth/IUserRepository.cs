using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories.Auth;

// User BaseEntity'den türemediği için generic IRepository<T> kullanılamıyor (A-03 tasarım kararı,
// CLAUDE.md §1) — Auth'a özel, dar kapsamlı arayüz. Update için ayrı bir metot YOK: çağıran Handler
// entity'yi bu arayüzden çekip mutasyona uğratır, EF change tracking zaten izliyor, SaveChangesAsync
// yeterli (OtpService/ILoginCompletionService ile aynı "servis saf mantık taşır" deseni).
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<User?> GetByAppleIdAsync(string appleId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
