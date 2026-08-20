using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Content;

public class CategoryRepository : ICategoryRepository
{
    private readonly ZauselDbContext _context;

    public CategoryRepository(ZauselDbContext context) => _context = context;

    public async Task<List<CategoryAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);
        var translationsByCategory = await BuildTranslationsAsync(categories.Select(c => c.Id).ToList(), cancellationToken);
        return categories.Select(c => new CategoryAggregate(c, translationsByCategory.GetValueOrDefault(c.Id, []))).ToList();
    }

    public async Task<CategoryAggregate?> GetAggregateAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category is null)
            return null;

        var translationsByCategory = await BuildTranslationsAsync([categoryId], cancellationToken);
        return new CategoryAggregate(category, translationsByCategory.GetValueOrDefault(categoryId, []));
    }

    public async Task<Dictionary<int, int>> GetActiveWordCountsAsync(CancellationToken cancellationToken = default)
    {
        var activeConceptIds = _context.WordConcepts.Where(c => c.IsActive).Select(c => c.Id);
        return await _context.WordCategories
            .Where(wc => activeConceptIds.Contains(wc.WordConceptId))
            .GroupBy(wc => wc.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(int categoryId, CancellationToken cancellationToken = default) =>
        await _context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);

    public async Task<bool> HasActiveWordsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var activeConceptIds = _context.WordConcepts.Where(c => c.IsActive).Select(c => c.Id);
        return await _context.WordCategories
            .Where(wc => wc.CategoryId == categoryId)
            .AnyAsync(wc => activeConceptIds.Contains(wc.WordConceptId), cancellationToken);
    }

    // categoryId == newParentCategoryId (sıfır uzunluklu döngü — kendi kendinin altına taşınma) DAHİL —
    // ICategoryRepository'deki yorumla AYNI gerekçe.
    public async Task<bool> WouldCreateCycleAsync(int categoryId, int newParentCategoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId == newParentCategoryId)
            return true;

        var parentById = await _context.Categories.ToDictionaryAsync(c => c.Id, c => c.ParentCategoryId, cancellationToken);

        int? currentId = newParentCategoryId;
        while (currentId is not null)
        {
            if (currentId == categoryId)
                return true;
            currentId = parentById.GetValueOrDefault(currentId.Value);
        }

        return false;
    }

    public async Task<bool> AllExistAsync(List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        var distinctIds = categoryIds.Distinct().ToList();
        if (distinctIds.Count == 0)
            return true;

        var existingCount = await _context.Categories.CountAsync(c => distinctIds.Contains(c.Id), cancellationToken);
        return existingCount == distinctIds.Count;
    }

    public async Task AddCategoryAsync(Category category, int? userId, CancellationToken cancellationToken = default)
    {
        category.CreatedByUserId = userId;
        category.UpdatedByUserId = userId;
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public Task UpdateCategoryAsync(Category category, int? userId, CancellationToken cancellationToken = default)
    {
        category.UpdatedByUserId = userId;
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteCategoryAsync(int categoryId, int? userId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            ?? throw new EntityNotFoundException($"Category not found: Id={categoryId}");

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedByUserId = userId;
        category.UpdatedByUserId = userId;
    }

    public async Task<CategoryTranslation?> FindTranslationAsync(int categoryId, int languageId, CancellationToken cancellationToken = default) =>
        await _context.CategoryTranslations.FirstOrDefaultAsync(
            t => t.CategoryId == categoryId && t.LanguageId == languageId, cancellationToken);

    public async Task AddTranslationAsync(CategoryTranslation translation, CancellationToken cancellationToken = default) =>
        await _context.CategoryTranslations.AddAsync(translation, cancellationToken);

    public Task UpdateTranslationAsync(CategoryTranslation translation, CancellationToken cancellationToken = default)
    {
        _context.CategoryTranslations.Update(translation);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    private async Task<Dictionary<int, List<CategoryTranslationAggregate>>> BuildTranslationsAsync(
        List<int> categoryIds, CancellationToken cancellationToken)
    {
        var translations = await _context.CategoryTranslations.Where(t => categoryIds.Contains(t.CategoryId)).ToListAsync(cancellationToken);
        var languages = await _context.Languages.ToDictionaryAsync(l => l.Id, cancellationToken: cancellationToken);

        return translations
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(t => new CategoryTranslationAggregate(t, languages[t.LanguageId])).ToList());
    }
}
