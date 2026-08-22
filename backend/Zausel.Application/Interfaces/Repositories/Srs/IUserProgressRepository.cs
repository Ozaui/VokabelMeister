using Zausel.Domain.Entities.Srs;

namespace Zausel.Application.Interfaces.Repositories.Srs;

public interface IUserProgressRepository
{
    // GetProgressSummaryQuery'nin bant/due sayımı için — UserCardProgressRepository'nin AYNI
    // metoduyla BİRLEŞTİRİLİR (bkz. ProgressSnapshot).
    Task<List<ProgressSnapshot>> GetSnapshotsAsync(int userId, CancellationToken cancellationToken = default);

    // maxExclusive=null → üst sınır yok ("İyi" bandı, Mastery hiçbir zaman 100'ü aşmaz).
    Task<List<WordProgressItem>> GetByMasteryRangeAsync(
        int userId, decimal minInclusive, decimal? maxExclusive, CancellationToken cancellationToken = default);

    Task<List<WordProgressItem>> GetSuspendedAsync(int userId, CancellationToken cancellationToken = default);

    // Leech aksiyonu (Suspend/Reset/Continue) için — sahiplik filtresi (UserId+WordId) gömülü.
    Task<UserProgress?> GetByUserAndWordAsync(int userId, int wordId, CancellationToken cancellationToken = default);

    // learn-system-word (A-10) için — bu, projenin bir UserProgress satırını İLK KEZ yarattığı yer
    // (A-09'daki tek Handler, ApplyWordLeechActionCommand, yalnızca VAR OLAN bir satırı günceller).
    Task AddAsync(UserProgress userProgress, int userId, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserProgress userProgress, int userId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
