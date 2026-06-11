/// <summary>
/// WordRepository.cs
///
/// AMAÇ: Sistem kelimesi sorgularının implementasyonu — çok boyutlu filtreleme ve ilişkili veri yükleme.
/// NEDEN: WordDetail/WordExample Include'ları ve SRS için yeni kelime sorgusu özel logic gerektirir.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IWordRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Vocabulary;

/// <summary>
/// Sistem kelimesi repository implementasyonu.
///
/// AMAÇ: IWordRepository sözleşmesini karşılamak.
/// NEDEN: Kelime listeleme, detay ve SRS için yeni kelime seçimi özel sorgular gerektirir.
/// </summary>
public class WordRepository : Repository<Word>, IWordRepository
{
    public WordRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Kelimeyi gramer detayları ve örnek cümlelerle getirir.
    /// NEDEN: Kart görüntüleme ekranı tek sorguda WordDetail + WordExamples ister.
    /// NASIL: Include zinciri ile eager loading; soft delete filter otomatik.
    /// </summary>
    public Task<Word?> GetWithDetailAsync(int id, CancellationToken ct = default)
        => _set
            .Include(w => w.WordDetail)
            .Include(w => w.WordExamples)
            .Include(w => w.WordCategories)
                .ThenInclude(wc => wc.Category)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    /// <summary>
    /// AMAÇ: Seviye, kategori, kelime türü ve metin aramasıyla sayfalı kelime listesi getirir.
    /// NEDEN: Admin paneli ve kelime listesi ekranı çok boyutlu filtre + sayfalama kullanır.
    /// NASIL: NULL parametre = filtre uygulanmaz; TotalCount paginasyon için ayrıca hesaplanır.
    /// </summary>
    public async Task<(IEnumerable<Word> Words, int TotalCount)> GetPagedAsync(
        string? level,
        int? categoryId,
        string? partOfSpeech,
        string? search,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = _set.AsQueryable();

        // Her null kontrol bağımsız bir filtre — koşullar AND ile birleşir
        if (!string.IsNullOrEmpty(level))
            query = query.Where(w => w.DifficultyLevel == level);

        if (!string.IsNullOrEmpty(partOfSpeech))
            query = query.Where(w => w.PartOfSpeech == partOfSpeech);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(w =>
                w.GermanWord.Contains(search) ||
                w.TurkishTranslation.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(w =>
                w.WordCategories.Any(wc => wc.CategoryId == categoryId.Value));

        var totalCount = await query.CountAsync(ct);

        var words = await query
            .OrderBy(w => w.GermanWord)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (words, totalCount);
    }

    /// <summary>
    /// AMAÇ: Belirli bir kategoriye ait kelimeleri sayfalı getirir.
    /// NEDEN: Kategori detay ekranı sadece o kategorinin kelimelerini gösterir.
    /// NASIL: WordCategories ara tablosu üzerinden CategoryId filtresi.
    /// </summary>
    public async Task<(IEnumerable<Word> Words, int TotalCount)> GetByCategoryAsync(
        int categoryId,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = _set.Where(w =>
            w.WordCategories.Any(wc => wc.CategoryId == categoryId));

        var totalCount = await query.CountAsync(ct);

        var words = await query
            .OrderBy(w => w.GermanWord)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (words, totalCount);
    }

    /// <summary>
    /// AMAÇ: Kullanıcının belirtilen seviyede hiç başlamadığı kelimeleri getirir.
    /// NEDEN: Günlük yeni kelime havuzu oluşturma — DailyNewWordLimit kadar yeni kelime seçilir.
    /// NASIL: UserProgresses tablosunda bu kullanıcı için kaydı olmayan kelimeler — LEFT JOIN + NULL kontrolü.
    /// </summary>
    public async Task<IEnumerable<Word>> GetNewWordsForUserAsync(
        int userId,
        string level,
        int count,
        CancellationToken ct = default)
        => await _set
            .Where(w =>
                w.DifficultyLevel == level &&
                w.IsActive &&
                !w.UserProgresses.Any(up => up.UserId == userId))
            .OrderBy(_ => Guid.NewGuid()) // rastgele sıralama
            .Take(count)
            .ToListAsync(ct);
}
