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

    public async Task<PagedResult<WordConcept>> GetPagedAsync(
        string? difficultyLevel,
        string? partOfSpeech,
        string? search,
        int? categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Include(c => c.WordCategories)
            .ThenInclude(wc => wc.Category)
            .ThenInclude(cat => cat.Translations)
            .ThenInclude(t => t.Language)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficultyLevel))
            query = query.Where(c => c.DifficultyLevel == difficultyLevel);
        if (!string.IsNullOrWhiteSpace(partOfSpeech))
            query = query.Where(c => c.PartOfSpeech == partOfSpeech);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Words.Any(w => w.Text.Contains(search)));
        if (categoryId is not null)
            query = query.Where(c => c.WordCategories.Any(wc => wc.CategoryId == categoryId.Value));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WordConcept>(items, totalCount, page, pageSize);
    }

    public Task<WordConcept?> GetWithTranslationsAsync(int id, CancellationToken ct = default) =>
        _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Include(c => c.Words)
            .ThenInclude(w => w.WordDetail)
            .Include(c => c.Words)
            .ThenInclude(w => w.WordExamples)
            .Include(c => c.WordCategories)
            .ThenInclude(wc => wc.Category)
            .ThenInclude(cat => cat.Translations)
            .ThenInclude(t => t.Language)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsWordTextAsync(int languageId, string text, CancellationToken ct = default) =>
        _db.Words.AnyAsync(w => w.LanguageId == languageId && w.Text == text, ct);

    // WordCategories de dahil edilir — silinmiş bir kavrama ait kategori bağı burada temizlenmezse
    // DB'de IsDeleted=false olarak yetim kalır ve her yeni sorgunun bunu ayrıca telafi etmesi gerekirdi.
    public async Task SoftDeleteWithWordsAsync(int id, int? userId, CancellationToken ct = default)
    {
        var concept =
            await _set.Include(c => c.Words).Include(c => c.WordCategories).FirstOrDefaultAsync(c => c.Id == id, ct)
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

        foreach (var wordCategory in concept.WordCategories)
        {
            wordCategory.IsDeleted = true;
            wordCategory.DeletedAt = now;
            wordCategory.DeletedByUserId = userId;
            wordCategory.UpdatedByUserId = userId;
        }

        await _db.SaveChangesAsync(ct);
    }

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

    // Sayfalanmaz — WordMatchSuggestionResolver öneri hesaplamak için tüm havuzu tarar.
    public async Task<IReadOnlyList<WordConcept>> GetUnmatchedOtherLanguagePoolAsync(
        int excludeLanguageId,
        CancellationToken ct = default
    ) =>
        await _set
            .Include(c => c.Words)
            .ThenInclude(w => w.Language)
            .Where(c => c.Words.Count == 1 && c.Words.Any(w => w.LanguageId != excludeLanguageId))
            .ToListAsync(ct);

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

    public Task<int> GetTotalCountAsync(CancellationToken ct = default) => _set.CountAsync(ct);
}
