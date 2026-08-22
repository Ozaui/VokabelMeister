using Microsoft.EntityFrameworkCore;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Srs;
using Zausel.Infrastructure.Data;

namespace Zausel.Infrastructure.Repositories.Srs;

public class UserProgressRepository : IUserProgressRepository
{
    private readonly ZauselDbContext _context;

    public UserProgressRepository(ZauselDbContext context) => _context = context;

    public async Task<List<ProgressSnapshot>> GetSnapshotsAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Silinmiş (IsDeleted) bir kelimenin ilerlemesi sayıma girmez — aksi halde bant/due sayıları,
        // GetByMasteryRangeAsync/GetSuspendedAsync'in (Word'e JOIN yaptığı için zaten hariç tuttuğu)
        // listelerle TUTARSIZ bir toplam gösterirdi.
        var activeWordIds = _context.Words.Select(w => w.Id);
        return await _context.UserProgress
            .Where(p => p.UserId == userId && activeWordIds.Contains(p.WordId))
            .Select(p => new ProgressSnapshot(p.Mastery, p.NextReviewAt, p.IsSuspended, p.CurrentLevel))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WordProgressItem>> GetByMasteryRangeAsync(
        int userId, decimal minInclusive, decimal? maxExclusive, CancellationToken cancellationToken = default)
    {
        var query = _context.UserProgress.Where(p => p.UserId == userId && p.Mastery >= minInclusive);
        if (maxExclusive.HasValue)
            query = query.Where(p => p.Mastery < maxExclusive.Value);

        var progressRows = await query.ToListAsync(cancellationToken);
        return await JoinWithWordsAsync(progressRows, cancellationToken);
    }

    public async Task<List<WordProgressItem>> GetSuspendedAsync(int userId, CancellationToken cancellationToken = default)
    {
        var progressRows = await _context.UserProgress
            .Where(p => p.UserId == userId && p.IsSuspended)
            .ToListAsync(cancellationToken);
        return await JoinWithWordsAsync(progressRows, cancellationToken);
    }

    public async Task<UserProgress?> GetByUserAndWordAsync(int userId, int wordId, CancellationToken cancellationToken = default) =>
        await _context.UserProgress.FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, cancellationToken);

    public async Task AddAsync(UserProgress userProgress, int userId, CancellationToken cancellationToken = default)
    {
        userProgress.CreatedByUserId = userId;
        userProgress.UpdatedByUserId = userId;
        await _context.UserProgress.AddAsync(userProgress, cancellationToken);
    }

    public Task UpdateAsync(UserProgress userProgress, int userId, CancellationToken cancellationToken = default)
    {
        userProgress.UpdatedByUserId = userId;
        _context.UserProgress.Update(userProgress);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    // Word navigation property'si yok (CategoryRepository'nin Language/Translation birleştirmesiyle
    // AYNI desen) — kelime metnini almak için ayrı bir sorgu + in-memory Dictionary eşleşmesi.
    private async Task<List<WordProgressItem>> JoinWithWordsAsync(List<UserProgress> progressRows, CancellationToken cancellationToken)
    {
        var wordIds = progressRows.Select(p => p.WordId).Distinct().ToList();
        var wordsById = await _context.Words.Where(w => wordIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, cancellationToken);

        return progressRows
            .Where(p => wordsById.ContainsKey(p.WordId))
            .Select(p =>
            {
                var word = wordsById[p.WordId];
                return new WordProgressItem(
                    p.WordId, word.Text, word.Definition, p.CurrentLevel, p.Mastery,
                    p.NextReviewAt, p.IsSuspended, p.ConsecutiveIncorrect);
            })
            .ToList();
    }
}
