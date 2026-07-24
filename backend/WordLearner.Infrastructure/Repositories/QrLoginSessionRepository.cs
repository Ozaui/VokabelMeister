using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class QrLoginSessionRepository : Repository<QrLoginSession>, IQrLoginSessionRepository
{
    public QrLoginSessionRepository(WordLearnerDbContext db)
        : base(db) { }

    public Task<QrLoginSession?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) => _set.FirstOrDefaultAsync(q => q.QrTokenHash == tokenHash, ct);
}
