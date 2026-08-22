using Microsoft.EntityFrameworkCore;
using Zausel.Application.DTOs;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.PersonalContent;

public class UserCardRepository : IUserCardRepository
{
    private readonly ZauselDbContext _context;

    public UserCardRepository(ZauselDbContext context) => _context = context;

    public async Task<PagedResult<UserCardAggregate>> GetPagedForUserAsync(
        int userId, int? categoryId, int? userCategoryId, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.UserCards.Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FrontText.Contains(search) || c.BackText.Contains(search));
        if (categoryId is not null)
        {
            var matchingCardIds = _context.UserCardCategories.Where(link => link.CategoryId == categoryId).Select(link => link.UserCardId);
            query = query.Where(c => matchingCardIds.Contains(c.Id));
        }
        if (userCategoryId is not null)
        {
            var matchingCardIds = _context.UserCardUserCategories.Where(link => link.UserCategoryId == userCategoryId).Select(link => link.UserCardId);
            query = query.Where(c => matchingCardIds.Contains(c.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var cards = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = await BuildAggregatesAsync(cards, cancellationToken);
        return new PagedResult<UserCardAggregate> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<UserCardAggregate?> GetByIdForUserAsync(int userCardId, int userId, CancellationToken cancellationToken = default)
    {
        var card = await _context.UserCards.FirstOrDefaultAsync(c => c.Id == userCardId && c.UserId == userId, cancellationToken);
        if (card is null)
            return null;

        var aggregates = await BuildAggregatesAsync([card], cancellationToken);
        return aggregates[0];
    }

    public async Task<UserCard?> FindByUserAndFrontTextAsync(int userId, string frontText, int? excludeUserCardId, CancellationToken cancellationToken = default)
    {
        var query = _context.UserCards.Where(c => c.UserId == userId && c.FrontText == frontText);
        if (excludeUserCardId is not null)
            query = query.Where(c => c.Id != excludeUserCardId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default)
    {
        userCard.CreatedByUserId = userId;
        userCard.UpdatedByUserId = userId;
        await _context.UserCards.AddAsync(userCard, cancellationToken);
    }

    public Task UpdateAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default)
    {
        userCard.UpdatedByUserId = userId;
        _context.UserCards.Update(userCard);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default)
    {
        userCard.IsDeleted = true;
        userCard.DeletedAt = DateTime.UtcNow;
        userCard.DeletedByUserId = userId;
        userCard.UpdatedByUserId = userId;
        return Task.CompletedTask;
    }

    public async Task ReplaceCategoriesAsync(int userCardId, List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserCardCategories.Where(link => link.UserCardId == userCardId).ToListAsync(cancellationToken);
        _context.UserCardCategories.RemoveRange(existing);

        var newRows = categoryIds.Distinct().Select(categoryId => new UserCardCategory
        {
            UserCardId = userCardId,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.UserCardCategories.AddRangeAsync(newRows, cancellationToken);
    }

    public async Task ReplaceUserCategoriesAsync(int userCardId, List<int> userCategoryIds, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserCardUserCategories.Where(link => link.UserCardId == userCardId).ToListAsync(cancellationToken);
        _context.UserCardUserCategories.RemoveRange(existing);

        var newRows = userCategoryIds.Distinct().Select(userCategoryId => new UserCardUserCategory
        {
            UserCardId = userCardId,
            UserCategoryId = userCategoryId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.UserCardUserCategories.AddRangeAsync(newRows, cancellationToken);
    }

    public async Task ReplaceExamplesAsync(int userCardId, List<UserCardExample> newExamples, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserCardExamples.Where(e => e.UserCardId == userCardId).ToListAsync(cancellationToken);
        _context.UserCardExamples.RemoveRange(existing);

        var now = DateTime.UtcNow;
        foreach (var example in newExamples)
        {
            example.CreatedAt = now;
            example.UpdatedAt = now;
        }

        await _context.UserCardExamples.AddRangeAsync(newExamples, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    private async Task<List<UserCardAggregate>> BuildAggregatesAsync(List<UserCard> cards, CancellationToken cancellationToken)
    {
        var cardIds = cards.Select(c => c.Id).ToList();

        var examplesByCard = (await _context.UserCardExamples
                .Where(e => cardIds.Contains(e.UserCardId))
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync(cancellationToken))
            .GroupBy(e => e.UserCardId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var categoryIdsByCard = (await _context.UserCardCategories
                .Where(link => cardIds.Contains(link.UserCardId))
                .ToListAsync(cancellationToken))
            .GroupBy(link => link.UserCardId)
            .ToDictionary(g => g.Key, g => g.Select(link => link.CategoryId).ToList());

        var userCategoryIdsByCard = (await _context.UserCardUserCategories
                .Where(link => cardIds.Contains(link.UserCardId))
                .ToListAsync(cancellationToken))
            .GroupBy(link => link.UserCardId)
            .ToDictionary(g => g.Key, g => g.Select(link => link.UserCategoryId).ToList());

        return cards
            .Select(c => new UserCardAggregate(
                c,
                examplesByCard.GetValueOrDefault(c.Id, []),
                categoryIdsByCard.GetValueOrDefault(c.Id, []),
                userCategoryIdsByCard.GetValueOrDefault(c.Id, [])))
            .ToList();
    }
}
