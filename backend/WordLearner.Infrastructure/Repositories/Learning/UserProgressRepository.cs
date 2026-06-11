/// <summary>
/// UserProgressRepository.cs
///
/// AMAÇ: Sistem kelimesi SRS ilerleme sorgularının implementasyonu.
/// NEDEN: SM-2 oturumu için tekrar sıralaması ve istatistik sorguları özel logic gerektirir.
/// BAĞIMLILIKLAR: IUserProgressRepository (Application), WordLearnerDbContext, UserProgress entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Learning;

/// <summary>
/// Sistem kelimesi SRS ilerleme repository implementasyonu.
///
/// AMAÇ: IUserProgressRepository sözleşmesini karşılamak.
/// NEDEN: UserProgress BaseEntity'den miras almaz — generic Repository kullanılamaz.
/// </summary>
public class UserProgressRepository : IUserProgressRepository
{
    private readonly WordLearnerDbContext _db;

    public UserProgressRepository(WordLearnerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// AMAÇ: Kullanıcı + kelime çifti için ilerleme kaydını getirir.
    /// NEDEN: SM-2 cevap işlenirken mevcut durum (CurrentLevel, EasinessFactor vb.) okunur.
    /// </summary>
    public Task<UserProgress?> GetByUserAndWordAsync(int userId, int wordId, CancellationToken ct = default)
        => _db.UserProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, ct);

    /// <summary>
    /// AMAÇ: Bugün tekrarı gelen kelimeleri getirir — SM-2 oturum listesi.
    /// NEDEN: NextReviewAt &lt;= bugün olan kayıtlar sıraya girer; en eskiden başlanır.
    /// NASIL: Word navigation property dahil edilir — kart görüntüleme için kelime verisi lazım.
    /// </summary>
    public async Task<IEnumerable<UserProgress>> GetDueForReviewAsync(
        int userId,
        int count,
        CancellationToken ct = default)
        => await _db.UserProgresses
            .Include(p => p.Word)
                .ThenInclude(w => w.WordDetail)
            .Where(p => p.UserId == userId && p.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(p => p.NextReviewAt) // en eski tekrar önce
            .Take(count)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: İlerleme kaydını günceller — SM-2 cevap işlemesinde çağrılır.
    /// NEDEN: UpdatedAt manuel güncellenir (UserProgress BaseEntity'den miras almıyor).
    /// </summary>
    public async Task UpdateAsync(UserProgress progress, CancellationToken ct = default)
    {
        progress.UpdatedAt = DateTime.UtcNow;
        _db.UserProgresses.Update(progress);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Yeni ilerleme kaydı oluşturur — kullanıcı kelimeyle ilk kez karşılaştığında.
    /// </summary>
    public async Task<UserProgress> AddAsync(UserProgress progress, CancellationToken ct = default)
    {
        await _db.UserProgresses.AddAsync(progress, ct);
        await _db.SaveChangesAsync(ct);
        return progress;
    }

    /// <summary>
    /// AMAÇ: Kullanıcının genel öğrenme istatistiklerini hesaplar.
    /// NEDEN: Profil/istatistik ekranı için özet metrik — toplam öğrenilen, mastered ve başarı oranı.
    /// NASIL: Tek sorguda gruplama ve agregasyon; CurrentLevel=5 mastery tanımı.
    /// </summary>
    public async Task<(int TotalLearned, int Mastered, double AverageSuccessRate)> GetStatisticsAsync(
        int userId,
        CancellationToken ct = default)
    {
        var progresses = await _db.UserProgresses
            .Where(p => p.UserId == userId && p.TotalAttempts > 0)
            .ToListAsync(ct);

        var totalLearned = progresses.Count;
        var mastered = progresses.Count(p => p.CurrentLevel >= 5);
        var averageSuccessRate = progresses.Count > 0
            ? progresses.Average(p => (double)p.SuccessRate)
            : 0.0;

        return (totalLearned, mastered, averageSuccessRate);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
