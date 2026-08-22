using Zausel.Application.DTOs;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;

namespace Zausel.Application.Interfaces.Repositories.Content;

// WordConcept BaseEntity'den türese de, tek başına bir "kelime" değil — Word/WordDetail/WordExample
// ile birlikte TEK bir aggregate oluşturuyor (bkz. Icerik.md). Bu yüzden generic IRepository<T>
// yalnızca WordConcept satırının kendisini yönetebilirdi, çevirileri/detayları/örnekleri AYRI AYRI
// yönetmek için Handler'ın 4 farklı repository enjekte etmesi gerekirdi — bunun yerine TEK bir
// aggregate-repository tüm graf üzerinde çalışıyor (User/RefreshToken'ın dar-arayüz deseniyle AYNI
// gerekçe: entity'nin doğası generic CRUD'a sığmıyor).
public interface IWordConceptRepository
{
    Task<Word?> FindWordByLanguageAndTextAsync(int languageId, string text, int? excludeWordId, CancellationToken cancellationToken = default);

    // UserCard'ın (A-10) "sistem eşleşmesi" önerisi için — kişisel kart hangi dilde yazıldığı
    // BİLİNMEDİĞİNDEN (UserCard'ın bir LanguageId'si yok) dile bakılmaksızın arar, FindWordByLanguageAndTextAsync'in
    // AKSİNE tek bir dille sınırlı DEĞİLDİR.
    Task<Word?> FindWordByTextAsync(string text, CancellationToken cancellationToken = default);
    Task<Word?> FindWordAsync(int wordConceptId, int languageId, CancellationToken cancellationToken = default);

    // learn-system-word (A-10) için — istemci UserProgress.WordId olarak KULLANILACAK somut Words.Id'yi
    // gönderir, Handler bu satırdan WordConceptId'yi öğrenip Almanca çeviriyi (FindWordAsync) ayrıca bulur.
    Task<Word?> GetWordByIdAsync(int wordId, CancellationToken cancellationToken = default);
    Task<WordDetail?> GetDetailByWordIdAsync(int wordId, CancellationToken cancellationToken = default);
    Task<WordConceptAggregate?> GetAggregateAsync(int wordConceptId, CancellationToken cancellationToken = default);

    Task<PagedResult<WordConceptAggregate>> GetPagedAsync(
        string? difficultyLevel, PartOfSpeech? partOfSpeech, string? search, int? categoryId,
        int page, int pageSize, CancellationToken cancellationToken = default);

    // Create/UpdateWordCommand'ın categoryIds[] alanı için — mevcut TÜM WordCategories satırlarını
    // (hard) siler, verilenlerle değiştirir. ReplaceExamplesAsync'in "translations[] tam değişim"
    // deseniyle AYNI (A-06 kullanıcı kararı — WordCategories'i boş bırakmamak için A-06'da eklendi).
    Task ReplaceWordCategoriesAsync(int wordConceptId, List<int> categoryIds, CancellationToken cancellationToken = default);

    // Eşleşmemiş = bir WordConcept'in TOPLAM tek Words satırı olması (dile bakılmaksızın) VE o
    // satırın languageId'de olması — ayrı bir IsMatched kolonu yok, durum COUNT(*)=1'den türetilir.
    Task<PagedResult<UnmatchedWordAggregate>> GetUnmatchedAsync(
        int languageId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    // GetUnmatchedAsync'in SAYFASIZ hâli — bir dildeki eşleşmemiş TÜM kelimelerin öneri havuzu
    // olarak kullanılması için (karşı dilin eşleşmemiş kavramlarına önerilecek aday listesi).
    Task<List<UnmatchedWordAggregate>> GetUnmatchedPoolAsync(int languageId, CancellationToken cancellationToken = default);

    Task AddConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default);
    Task UpdateConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default);
    Task SoftDeleteConceptCascadeAsync(int wordConceptId, int? userId, CancellationToken cancellationToken = default);

    // Eşleştirme (PairWordConceptsCommand) iki adımlı: önce otherConceptId'nin Word'leri
    // primaryId'ye TAŞINIR (MoveWordToConceptAsync), sonra artık boş kalan otherConceptId
    // SoftDeleteConceptOnlyAsync ile silinir — SoftDeleteConceptCascadeAsync'in AKSİNE Words'e
    // DOKUNMAZ, çünkü onlar zaten başka bir kavrama taşındı.
    Task MoveWordToConceptAsync(int wordId, int targetConceptId, int? userId, CancellationToken cancellationToken = default);
    Task SoftDeleteConceptOnlyAsync(int wordConceptId, int? userId, CancellationToken cancellationToken = default);

    Task AddWordAsync(Word word, int? userId, CancellationToken cancellationToken = default);
    Task UpdateWordAsync(Word word, int? userId, CancellationToken cancellationToken = default);
    Task AddDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default);
    Task UpdateDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default);
    Task ReplaceExamplesAsync(int wordId, List<WordExample> newExamples, int? userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
