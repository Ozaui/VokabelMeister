using Zausel.Domain.Entities.Content;

namespace Zausel.Application.Interfaces.Repositories.Content;

// WordConceptRepository ile AYNI aggregate-repository deseni — Category tek başına, çevirisi
// olmadan anlamsız (bkz. CategoryAggregate).
public interface ICategoryRepository
{
    Task<List<CategoryAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryAggregate?> GetAggregateAsync(int categoryId, CancellationToken cancellationToken = default);

    // includeWordCount=true isteyen GetCategoriesQuery için — CategoryId → o kategorideki AKTİF
    // (silinmemiş) WordConcept sayısı, WordCategories üzerinden.
    Task<Dictionary<int, int>> GetActiveWordCountsAsync(CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveWordsAsync(int categoryId, CancellationToken cancellationToken = default);

    // categoryId == newParentCategoryId (kendi kendinin altına taşınma) DAHİL kabul edilir — sıfır
    // uzunluklu bir döngü de bir döngüdür, ayrı bir "self-parent" hata kodu AÇILMAZ.
    Task<bool> WouldCreateCycleAsync(int categoryId, int newParentCategoryId, CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(List<int> categoryIds, CancellationToken cancellationToken = default);

    Task AddCategoryAsync(Category category, int? userId, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Category category, int? userId, CancellationToken cancellationToken = default);
    Task SoftDeleteCategoryAsync(int categoryId, int? userId, CancellationToken cancellationToken = default);

    Task<CategoryTranslation?> FindTranslationAsync(int categoryId, int languageId, CancellationToken cancellationToken = default);
    Task AddTranslationAsync(CategoryTranslation translation, CancellationToken cancellationToken = default);
    Task UpdateTranslationAsync(CategoryTranslation translation, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
