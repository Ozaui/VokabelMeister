// ─────────────────────────────────────────────────────────────────────────────
// IWordConceptRepository.cs
//
// AMAÇ: WordConcept aggregate root'una özel sorgular — sayfalı liste, tüm
//       dilleriyle detay, duplikat kontrolü, kavram+diller birlikte soft delete.
// NEDEN: Word/WordDetail/WordExample için AYRI top-level repository AÇILMAZ —
//        hepsi bu repository üzerinden (DbContext'e Include zinciriyle) child
//        olarak yönetilir; aggregate root deseni, ayrı repository'ler gerçek
//        bir tüketici olmadan spekülatif olurdu (YAGNI, bkz. A-05 planı).
// BAĞIMLILIKLAR: IRepository<WordConcept>, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IWordConceptRepository : IRepository<WordConcept>
{
    // AMAÇ: Liste ekranı için filtre+sayfa — her kavramın dillerini (yalnızca
    //       Text seviyesinde, WordDetail/WordExample olmadan) de yükler.
    Task<PagedResult<WordConcept>> GetPagedAsync(
        string? difficultyLevel,
        string? partOfSpeech,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    // AMAÇ: Detay/güncelleme ekranı için tüm dilleri + WordDetail + WordExample'larıyla yükler.
    Task<WordConcept?> GetWithTranslationsAsync(int id, CancellationToken ct = default);

    // AMAÇ: Bir dilde aynı Text'e sahip başka bir Word olup olmadığını kontrol eder
    //       (duplikat 409 + ?force=true kararı için).
    Task<bool> ExistsWordTextAsync(int languageId, string text, CancellationToken ct = default);

    // AMAÇ: WordConcept + ona bağlı TÜM Word satırlarını tek işlemde soft-delete eder.
    // NEDEN: Repository<T>.SoftDeleteAsync yalnızca WordConcept'in kendisini işaretler —
    //        Word'ler ayrı bir DbSet olduğu için parent silinince child'ların aktif
    //        kalmaması bu metotla garanti edilir (bkz. A-05 planı karar #2).
    Task SoftDeleteWithWordsAsync(int id, int? userId, CancellationToken ct = default);

    // AMAÇ: `languageId`'de eşleşmemiş (tam olarak 1 Word'ü olan) kavramların
    //       filtre+sayfalı listesi — `GET /words/unmatched` (bkz. Icerik.md "Eşleştirme").
    Task<PagedResult<WordConcept>> GetUnmatchedPagedAsync(
        int languageId,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    );

    // AMAÇ: `excludeLanguageId` DIŞINDAKİ dillerde eşleşmemiş kavramların TAMAMI —
    //       WordMatchSuggestionResolver'ın öneri üretmek için taradığı havuz
    //       (sayfalanmaz, öneri hesaplaması tüm havuza karşı yapılmalı).
    Task<IReadOnlyList<WordConcept>> GetUnmatchedOtherLanguagePoolAsync(
        int excludeLanguageId,
        CancellationToken ct = default
    );

    // AMAÇ: `otherConceptId`'nin tek Word'ünü `primaryId`'ye taşır, boş kalan
    //       `otherConceptId`'yi soft-delete eder, birleşmiş `primaryId`'yi döner.
    // NEDEN: Bloklayıcı hata yok (Icerik.md "Eşleştirme") — PartOfSpeech/Category/
    //        DifficultyLevel çakışsa bile primaryId'ninki sessizce kazanır.
    Task<WordConcept> PairAsync(int primaryId, int otherConceptId, int? userId, CancellationToken ct = default);
}
