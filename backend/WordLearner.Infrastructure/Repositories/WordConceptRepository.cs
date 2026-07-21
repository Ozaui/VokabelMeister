// ─────────────────────────────────────────────────────────────────────────────
// WordConceptRepository.cs
//
// AMAÇ: IWordConceptRepository'nin EF Core implementasyonu.
// NEDEN: Repository<WordConcept>'i miras alarak genel CRUD'u yeniden yazmadan
//        yalnızca WordConcept aggregate'ine özgü sorguları ekler.
// BAĞIMLILIKLAR: EF Core, Repository<WordConcept>, WordLearnerDbContext,
//                WordConcept/Word entity'leri, PagedResult<T>, EntityNotFoundException.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class WordConceptRepository : Repository<WordConcept>, IWordConceptRepository
{
    public WordConceptRepository(WordLearnerDbContext db)
        : base(db) { }

    // AMAÇ: Liste ekranı — dil bazında yalnızca Word.Text seviyesinde Include (WordDetail/
    //       WordExample YOK, liste satırı bu kadar detay göstermiyor, gereksiz sorgu yükünden kaçınılır).
    public async Task<PagedResult<WordConcept>> GetPagedAsync(
        string? difficultyLevel,
        string? partOfSpeech,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _set.Include(c => c.Words).ThenInclude(w => w.Language).AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficultyLevel))
            query = query.Where(c => c.DifficultyLevel == difficultyLevel);
        if (!string.IsNullOrWhiteSpace(partOfSpeech))
            query = query.Where(c => c.PartOfSpeech == partOfSpeech);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Words.Any(w => w.Text.Contains(search)));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WordConcept>(items, totalCount, page, pageSize);
    }

    // AMAÇ: Detay/güncelleme — her dilin WordDetail (gramer) ve WordExample'larıyla birlikte yükler.
    public Task<WordConcept?> GetWithTranslationsAsync(int id, CancellationToken ct = default) =>
        _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Include(c => c.Words)
            .ThenInclude(w => w.WordDetail)
            .Include(c => c.Words)
            .ThenInclude(w => w.WordExamples)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    // AMAÇ: Aynı dilde aynı Text'e sahip başka bir Word var mı — WordConcept üzerinden
    //       değil doğrudan Words DbSet'i üzerinden sorgular (soft-delete filtresi otomatik uygulanır).
    public Task<bool> ExistsWordTextAsync(int languageId, string text, CancellationToken ct = default) =>
        _db.Words.AnyAsync(w => w.LanguageId == languageId && w.Text == text, ct);

    // AMAÇ: WordConcept + tüm Word'lerini TEK transaction'da (SaveChangesAsync tek çağrı) soft-delete eder.
    public async Task SoftDeleteWithWordsAsync(int id, int? userId, CancellationToken ct = default)
    {
        var concept =
            await _set.Include(c => c.Words).FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), id);

        var now = DateTime.UtcNow;

        concept.IsDeleted = true;
        concept.DeletedAt = now;
        concept.DeletedByUserId = userId;
        concept.UpdatedByUserId = userId;

        foreach (var word in concept.Words)
        {
            word.IsDeleted = true;
            word.DeletedAt = now;
            word.DeletedByUserId = userId;
            word.UpdatedByUserId = userId;
        }

        await _db.SaveChangesAsync(ct);
    }

    // AMAÇ: Liste ekranı — `languageId`'de tam olarak 1 Word'ü olan (eşleşmemiş) kavramlar.
    public async Task<PagedResult<WordConcept>> GetUnmatchedPagedAsync(
        int languageId,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Where(c => c.Words.Count == 1 && c.Words.Any(w => w.LanguageId == languageId));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Words.Any(w => w.Text.Contains(search)));

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<WordConcept>(items, totalCount, page, pageSize);
    }

    // AMAÇ: Öneri hesaplaması için `excludeLanguageId` DIŞINDAKİ dillerde eşleşmemiş
    //       TÜM kavramlar (sayfalanmaz — WordMatchSuggestionResolver tüm havuzu tarar).
    public async Task<IReadOnlyList<WordConcept>> GetUnmatchedOtherLanguagePoolAsync(
        int excludeLanguageId,
        CancellationToken ct = default
    ) =>
        await _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Where(c => c.Words.Count == 1 && c.Words.Any(w => w.LanguageId != excludeLanguageId))
            .ToListAsync(ct);

    // AMAÇ: `otherConceptId`'nin tek Word'ünü `primaryId`'ye taşır, boş kalan
    //       `otherConceptId`'yi soft-delete eder — tek SaveChangesAsync ile.
    public async Task<WordConcept> PairAsync(
        int primaryId,
        int otherConceptId,
        int? userId,
        CancellationToken ct = default
    )
    {
        var primary =
            await GetWithTranslationsAsync(primaryId, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), primaryId);
        var other =
            await GetWithTranslationsAsync(otherConceptId, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), otherConceptId);

        var otherWord = other.Words.Single();
        other.Words.Remove(otherWord);
        otherWord.WordConceptId = primary.Id;
        otherWord.UpdatedByUserId = userId;
        primary.Words.Add(otherWord);

        var now = DateTime.UtcNow;
        other.IsDeleted = true;
        other.DeletedAt = now;
        other.DeletedByUserId = userId;
        other.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);

        return primary;
    }
}
