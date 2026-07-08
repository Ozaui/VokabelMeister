// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSessionRepository.cs
//
// AMAÇ: IQrLoginSessionRepository'nin EF Core implementasyonu.
// NEDEN: Repository<T>'yi miras alarak genel CRUD'u yeniden yazmadan yalnızca
//        QrLoginSession'a özgü hash aramasını ekler.
// BAĞIMLILIKLAR: EF Core, Repository<T>, WordLearnerDbContext, QrLoginSession entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class QrLoginSessionRepository : Repository<QrLoginSession>, IQrLoginSessionRepository
{
    public QrLoginSessionRepository(WordLearnerDbContext db)
        : base(db) { }

    // AMAÇ: SHA-256 hash'ine göre QR oturum kaydını bulur.
    public Task<QrLoginSession?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) => _set.FirstOrDefaultAsync(q => q.QrTokenHash == tokenHash, ct);
}
