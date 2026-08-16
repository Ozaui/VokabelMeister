using WordLearner.Application.DTOs;
using WordLearner.Domain.Entities.Content;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Application.Interfaces.Repositories.Content;

// WordConcept BaseEntity'den türese de, tek başına bir "kelime" değil — Word/WordDetail/WordExample
// ile birlikte TEK bir aggregate oluşturuyor (bkz. Icerik.md). Bu yüzden generic IRepository<T>
// yalnızca WordConcept satırının kendisini yönetebilirdi, çevirileri/detayları/örnekleri AYRI AYRI
// yönetmek için Handler'ın 4 farklı repository enjekte etmesi gerekirdi — bunun yerine TEK bir
// aggregate-repository tüm graf üzerinde çalışıyor (User/RefreshToken'ın dar-arayüz deseniyle AYNI
// gerekçe: entity'nin doğası generic CRUD'a sığmıyor).
public interface IWordConceptRepository
{
    Task<Word?> FindWordByLanguageAndTextAsync(int languageId, string text, int? excludeWordId, CancellationToken cancellationToken = default);
    Task<Word?> FindWordAsync(int wordConceptId, int languageId, CancellationToken cancellationToken = default);
    Task<WordDetail?> GetDetailByWordIdAsync(int wordId, CancellationToken cancellationToken = default);
    Task<WordConceptAggregate?> GetAggregateAsync(int wordConceptId, CancellationToken cancellationToken = default);

    Task<PagedResult<WordConceptAggregate>> GetPagedAsync(
        string? difficultyLevel, PartOfSpeech? partOfSpeech, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default);
    Task UpdateConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default);
    Task SoftDeleteConceptCascadeAsync(int wordConceptId, int? userId, CancellationToken cancellationToken = default);

    Task AddWordAsync(Word word, int? userId, CancellationToken cancellationToken = default);
    Task UpdateWordAsync(Word word, int? userId, CancellationToken cancellationToken = default);
    Task AddDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default);
    Task UpdateDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default);
    Task ReplaceExamplesAsync(int wordId, List<WordExample> newExamples, int? userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
