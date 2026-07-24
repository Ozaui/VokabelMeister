using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IQrLoginSessionRepository : IRepository<QrLoginSession>
{
    Task<QrLoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
}
