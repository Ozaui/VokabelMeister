using Microsoft.EntityFrameworkCore;
using WordLearner.Application.DTOs;
using WordLearner.Application.Interfaces.Repositories.Content;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Entities.Content;
using WordLearner.Domain.Enums.Content;
using WordLearner.Domain.Exceptions;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Content;

public class WordConceptRepository : IWordConceptRepository
{
    private readonly WordLearnerDbContext _context;

    public WordConceptRepository(WordLearnerDbContext context) => _context = context;

    public async Task<Word?> FindWordByLanguageAndTextAsync(int languageId, string text, int? excludeWordId, CancellationToken cancellationToken = default)
    {
        var query = _context.Words.Where(w => w.LanguageId == languageId && w.Text == text);
        if (excludeWordId is not null)
            query = query.Where(w => w.Id != excludeWordId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Word?> FindWordAsync(int wordConceptId, int languageId, CancellationToken cancellationToken = default) =>
        await _context.Words.FirstOrDefaultAsync(w => w.WordConceptId == wordConceptId && w.LanguageId == languageId, cancellationToken);

    public async Task<WordDetail?> GetDetailByWordIdAsync(int wordId, CancellationToken cancellationToken = default) =>
        await _context.WordDetails.FirstOrDefaultAsync(d => d.WordId == wordId, cancellationToken);

    public async Task<WordConceptAggregate?> GetAggregateAsync(int wordConceptId, CancellationToken cancellationToken = default)
    {
        var concept = await _context.WordConcepts.FirstOrDefaultAsync(c => c.Id == wordConceptId, cancellationToken);
        if (concept is null)
            return null;

        var translationsByConcept = await BuildTranslationsAsync([wordConceptId], cancellationToken);
        return new WordConceptAggregate(concept, translationsByConcept.GetValueOrDefault(wordConceptId, []));
    }

    public async Task<PagedResult<WordConceptAggregate>> GetPagedAsync(
        string? difficultyLevel, PartOfSpeech? partOfSpeech, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.WordConcepts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficultyLevel))
            query = query.Where(c => c.DifficultyLevel == difficultyLevel);
        if (partOfSpeech is not null)
            query = query.Where(c => c.PartOfSpeech == partOfSpeech);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var matchingConceptIds = _context.Words
                .Where(w => w.Text.Contains(search) || (w.Definition != null && w.Definition.Contains(search)))
                .Select(w => w.WordConceptId);
            query = query.Where(c => matchingConceptIds.Contains(c.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var concepts = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var conceptIds = concepts.Select(c => c.Id).ToList();
        var translationsByConcept = await BuildTranslationsAsync(conceptIds, cancellationToken);

        var items = concepts
            .Select(c => new WordConceptAggregate(c, translationsByConcept.GetValueOrDefault(c.Id, [])))
            .ToList();

        return new PagedResult<WordConceptAggregate> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task AddConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default)
    {
        concept.CreatedByUserId = userId;
        concept.UpdatedByUserId = userId;
        await _context.WordConcepts.AddAsync(concept, cancellationToken);
    }

    public Task UpdateConceptAsync(WordConcept concept, int? userId, CancellationToken cancellationToken = default)
    {
        concept.UpdatedByUserId = userId;
        _context.WordConcepts.Update(concept);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteConceptCascadeAsync(int wordConceptId, int? userId, CancellationToken cancellationToken = default)
    {
        var concept = await _context.WordConcepts.FirstOrDefaultAsync(c => c.Id == wordConceptId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={wordConceptId}");

        var words = await _context.Words.Where(w => w.WordConceptId == wordConceptId).ToListAsync(cancellationToken);
        var wordIds = words.Select(w => w.Id).ToList();
        var details = await _context.WordDetails.Where(d => wordIds.Contains(d.WordId)).ToListAsync(cancellationToken);
        var examples = await _context.WordExamples.Where(e => wordIds.Contains(e.WordId)).ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var entity in new BaseEntity[] { concept }.Concat(words).Concat(details).Concat(examples))
        {
            entity.IsDeleted = true;
            entity.DeletedAt = now;
            entity.DeletedByUserId = userId;
            entity.UpdatedByUserId = userId;
        }
    }

    public async Task AddWordAsync(Word word, int? userId, CancellationToken cancellationToken = default)
    {
        word.CreatedByUserId = userId;
        word.UpdatedByUserId = userId;
        await _context.Words.AddAsync(word, cancellationToken);
    }

    public Task UpdateWordAsync(Word word, int? userId, CancellationToken cancellationToken = default)
    {
        word.UpdatedByUserId = userId;
        _context.Words.Update(word);
        return Task.CompletedTask;
    }

    public async Task AddDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default)
    {
        detail.CreatedByUserId = userId;
        detail.UpdatedByUserId = userId;
        await _context.WordDetails.AddAsync(detail, cancellationToken);
    }

    public Task UpdateDetailAsync(WordDetail detail, int? userId, CancellationToken cancellationToken = default)
    {
        detail.UpdatedByUserId = userId;
        _context.WordDetails.Update(detail);
        return Task.CompletedTask;
    }

    public async Task ReplaceExamplesAsync(int wordId, List<WordExample> newExamples, int? userId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WordExamples.Where(e => e.WordId == wordId).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var example in existing)
        {
            example.IsDeleted = true;
            example.DeletedAt = now;
            example.DeletedByUserId = userId;
            example.UpdatedByUserId = userId;
        }

        foreach (var example in newExamples)
        {
            example.CreatedByUserId = userId;
            example.UpdatedByUserId = userId;
        }

        await _context.WordExamples.AddRangeAsync(newExamples, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    private async Task<Dictionary<int, List<WordTranslationAggregate>>> BuildTranslationsAsync(
        List<int> wordConceptIds, CancellationToken cancellationToken)
    {
        var words = await _context.Words.Where(w => wordConceptIds.Contains(w.WordConceptId)).ToListAsync(cancellationToken);
        var wordIds = words.Select(w => w.Id).ToList();

        var languages = await _context.Languages.ToDictionaryAsync(l => l.Id, cancellationToken: cancellationToken);
        var details = await _context.WordDetails.Where(d => wordIds.Contains(d.WordId)).ToDictionaryAsync(d => d.WordId, cancellationToken: cancellationToken);
        var examplesByWord = (await _context.WordExamples.Where(e => wordIds.Contains(e.WordId)).ToListAsync(cancellationToken))
            .ToLookup(e => e.WordId);

        return words
            .GroupBy(w => w.WordConceptId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(w => new WordTranslationAggregate(
                    w, languages[w.LanguageId], details.GetValueOrDefault(w.Id), examplesByWord[w.Id].ToList()
                )).ToList());
    }
}
