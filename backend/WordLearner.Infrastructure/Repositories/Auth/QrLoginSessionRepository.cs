using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Auth;

public class QrLoginSessionRepository : IQrLoginSessionRepository
{
    private readonly WordLearnerDbContext _context;

    public QrLoginSessionRepository(WordLearnerDbContext context) => _context = context;

    public async Task<QrLoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await _context.QrLoginSessions.FirstOrDefaultAsync(x => x.QrTokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(QrLoginSession session, CancellationToken cancellationToken = default) =>
        await _context.QrLoginSessions.AddAsync(session, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
