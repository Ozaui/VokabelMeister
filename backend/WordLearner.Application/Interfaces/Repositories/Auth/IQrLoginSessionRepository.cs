using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories.Auth;

// QrLoginSession BaseEntity'den türüyor ama token-hash'e göre arama generic IRepository<T>'de yok —
// User/RefreshToken repository'leriyle aynı dar-arayüz deseni (Update metodu yok, EF change tracking
// zaten izliyor; Scan/Confirm/Deny/GetStatus fetch ettiği entity'yi mutasyona uğratır, SaveChangesAsync yeterli).
public interface IQrLoginSessionRepository
{
    Task<QrLoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(QrLoginSession session, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
