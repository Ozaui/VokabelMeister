using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Content;

public class LanguageRepository : ILanguageRepository
{
    private readonly ZauselDbContext _context;

    public LanguageRepository(ZauselDbContext context) => _context = context;

    public async Task<List<Language>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.Languages.FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
}
