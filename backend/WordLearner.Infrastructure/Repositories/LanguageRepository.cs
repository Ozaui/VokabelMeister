using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

// Repository<T>'yi MİRAS ALMAZ — Language BaseEntity'den türemiyor.
public class LanguageRepository : ILanguageRepository
{
    private readonly WordLearnerDbContext _db;

    public LanguageRepository(WordLearnerDbContext db) => _db = db;

    public Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _db.Languages.FirstOrDefaultAsync(l => l.Code == code, ct);

    public Task<Language?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Languages.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<Language>> GetAllActiveAsync(CancellationToken ct = default) =>
        await _db.Languages.Where(l => l.IsActive).OrderBy(l => l.DisplayOrder).ToListAsync(ct);
}
