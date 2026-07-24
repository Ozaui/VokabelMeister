using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    // Ağaç, düz listeden BELLEKTE (CategoryDtoBuilder) kurulur — sınırsız derinlikli bir
    // hiyerarşiyi tek sorguda Include zinciriyle çekmenin EF Core'da doğrudan yolu yok.
    // level filtresi de SQL'de DEĞİL bellekte uygulanır — string.Compare SQL Server'a güvenilir çevrilmez.
    Task<IReadOnlyList<Category>> GetAllWithTranslationsAsync(string? level, CancellationToken ct = default);

    Task<Category?> GetWithTranslationsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyDictionary<int, int>> GetWordCountsAsync(CancellationToken ct = default);
    Task<bool> HasChildrenAsync(int categoryId, CancellationToken ct = default);
    Task<bool> HasActiveWordsAsync(int categoryId, CancellationToken ct = default);
    Task<bool> WouldCreateCycleAsync(int categoryId, int newParentId, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}
