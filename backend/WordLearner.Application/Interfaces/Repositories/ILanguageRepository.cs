using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Interfaces.Repositories;

// Language BaseEntity'den TÜREMEDİĞİ için IRepository<T> kullanamaz — bespoke, küçük bir arayüz.
public interface ILanguageRepository
{
    Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Language?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Language>> GetAllActiveAsync(CancellationToken ct = default);
}
