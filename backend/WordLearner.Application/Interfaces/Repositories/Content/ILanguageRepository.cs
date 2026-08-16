using WordLearner.Domain.Entities.Content;

namespace WordLearner.Application.Interfaces.Repositories.Content;

// Language BaseEntity'den türemediği için generic IRepository<T> kullanılamıyor — salt-okur, dar
// kapsamlı arayüz (yazma yolu yok, CRUD'u yok, seed migration-zamanında oluşuyor).
public interface ILanguageRepository
{
    Task<List<Language>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);
    Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
