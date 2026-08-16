using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories.Content;
using WordLearner.Domain.Entities.Content;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Content;

public class LanguageRepository : ILanguageRepository
{
    private readonly WordLearnerDbContext _context;

    public LanguageRepository(WordLearnerDbContext context) => _context = context;

    public async Task<List<Language>> GetActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.Languages.FirstOrDefaultAsync(l => l.Code == code, cancellationToken);
}
