/// <summary>
/// IWordRepository.cs
///
/// AMAÇ: Sistem kelimesi sorgularına özgü repository sözleşmesi.
/// NEDEN: Çok boyutlu filtreleme (seviye, kategori, tür, arama) ve ilişkili veri yükleme gerekir.
/// BAĞIMLILIKLAR: IRepository (generic), Word entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Sistem kelimesi repository arayüzü.
///
/// AMAÇ: Kelime listeleme, detay ve admin operasyonlarını tanımlamak.
/// NEDEN: WordDetail ve WordExample ilişkileri explicit Include gerektiriyor.
/// </summary>
public interface IWordRepository : IRepository<Word>
{
    /// <summary>
    /// AMAÇ: Kelimeyi gramer detayları ve örnek cümlelerle birlikte getirir.
    /// NEDEN: Kart görüntüleme ekranı tek sorguda tüm bilgilere ihtiyaç duyar.
    /// NASIL: WordDetail ve WordExamples eager load edilir.
    /// </summary>
    Task<Word?> GetWithDetailAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Çok boyutlu filtreyle kelime listesi getirir — sayfalama destekli.
    /// NEDEN: Admin paneli ve kullanıcı kelime listesi farklı filtre kombinasyonları kullanır.
    /// NASIL: NULL parametre = filtre uygulanmaz.
    /// </summary>
    Task<(IEnumerable<Word> Words, int TotalCount)> GetPagedAsync(
        string? level,
        int? categoryId,
        string? partOfSpeech,
        string? search,
        int page,
        int size,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Belirli bir kategoriye ait kelimeleri getirir — sayfalama destekli.
    /// NEDEN: Kategori detay ekranı sadece o kategorinin kelimelerini gösterir.
    /// </summary>
    Task<(IEnumerable<Word> Words, int TotalCount)> GetByCategoryAsync(
        int categoryId,
        int page,
        int size,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Belirli seviyedeki, kullanıcının henüz başlamadığı kelimeleri getirir.
    /// NEDEN: SRS oturumu için günlük yeni kelime havuzu oluşturmada kullanılır.
    /// NASIL: UserProgress tablosunda UserId ile kaydı olmayan kelimeler filtrelenir.
    /// </summary>
    Task<IEnumerable<Word>> GetNewWordsForUserAsync(
        int userId,
        string level,
        int count,
        CancellationToken ct = default);
}
