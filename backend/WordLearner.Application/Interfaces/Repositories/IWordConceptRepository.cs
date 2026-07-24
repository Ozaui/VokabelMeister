using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Interfaces.Repositories;

// Word/WordDetail/WordExample için AYRI top-level repository AÇILMAZ — hepsi bu aggregate
// root üzerinden (Include zinciriyle) child olarak yönetilir.
public interface IWordConceptRepository : IRepository<WordConcept>
{
    // GetCategoryWordsQuery de AYNI metodu kullanır (categoryId dolu, diğerleri null).
    Task<PagedResult<WordConcept>> GetPagedAsync(
        string? difficultyLevel,
        string? partOfSpeech,
        string? search,
        int? categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    Task<WordConcept?> GetWithTranslationsAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsWordTextAsync(int languageId, string text, CancellationToken ct = default);

    // WordConcept + tüm Word'lerini tek işlemde soft-delete eder — Repository<T>.SoftDeleteAsync
    // yalnızca WordConcept'in kendisini işaretler, child Word'ler ayrı DbSet olduğu için bu gerekli.
    Task SoftDeleteWithWordsAsync(int id, int? userId, CancellationToken ct = default);

    Task<PagedResult<WordConcept>> GetUnmatchedPagedAsync(
        int languageId,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    // Sayfalanmaz — WordMatchSuggestionResolver öneri üretmek için tüm havuzu tarar.
    Task<IReadOnlyList<WordConcept>> GetUnmatchedOtherLanguagePoolAsync(
        int excludeLanguageId,
        CancellationToken ct = default
    );

    Task<WordConcept> PairAsync(int primaryId, int otherConceptId, int? userId, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}
