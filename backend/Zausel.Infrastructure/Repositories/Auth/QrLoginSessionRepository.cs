using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Domain.Entities.Auth;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Auth;

public class QrLoginSessionRepository : IQrLoginSessionRepository
{
    private readonly ZauselDbContext _context;

    public QrLoginSessionRepository(ZauselDbContext context) => _context = context;

    public async Task<QrLoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await _context.QrLoginSessions.FirstOrDefaultAsync(x => x.QrTokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(QrLoginSession session, CancellationToken cancellationToken = default) =>
        await _context.QrLoginSessions.AddAsync(session, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
