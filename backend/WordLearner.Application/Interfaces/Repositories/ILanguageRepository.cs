// ─────────────────────────────────────────────────────────────────────────────
// ILanguageRepository.cs
//
// AMAÇ: Language'a özel sorgular — kod ile arama, aktif dilleri listeleme.
// NEDEN: `Language` `BaseEntity`'den TÜREMEDİĞİ için (bkz. Language.cs "NEDEN")
//        `IRepository<T>`'yi kullanamaz — bu bespoke, küçük bir arayüz.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.Words.Language.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ILanguageRepository
{
    // AMAÇ: ISO 639-1 koduna (ör. "de") göre dili bulur — CreateWordCommand/
    //       UpdateWordCommand `translations[].languageCode`'u bu şekilde çözer.
    Task<Language?> GetByCodeAsync(string code, CancellationToken ct = default);

    // AMAÇ: Birincil anahtara göre dili bulur.
    Task<Language?> GetByIdAsync(int id, CancellationToken ct = default);

    // AMAÇ: Aktif tüm dilleri görüntüleme sırasına göre döner (ör. admin panel dropdown'u).
    Task<IReadOnlyList<Language>> GetAllActiveAsync(CancellationToken ct = default);
}
