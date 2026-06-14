/// <summary>
/// UserCardProgressRepository.cs
///
/// AMAÇ: Kişisel kart SRS ilerleme sorgularının implementasyonu.
/// NEDEN: GetOrCreateAsync deseni kişisel kartların ilk görüşünü otomatik başlatır.
/// BAĞIMLILIKLAR: IUserCardProgressRepository (Application), WordLearnerDbContext, UserCardProgress entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Learning;

/// <summary>
/// Kişisel kart SRS ilerleme repository implementasyonu.
///
/// AMAÇ: IUserCardProgressRepository sözleşmesini karşılamak.
/// NEDEN: UserCardProgress BaseEntity'den miras almaz — generic Repository kullanılamaz.
/// </summary>
public class UserCardProgressRepository : IUserCardProgressRepository
{
    private readonly WordLearnerDbContext _db;

    public UserCardProgressRepository(WordLearnerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// AMAÇ: Belirli bir kart için ilerleme kaydını getirir.
    /// </summary>
    public Task<UserCardProgress?> GetByUserCardAsync(int userCardId, CancellationToken ct = default)
        => _db.UserCardProgresses
            .FirstOrDefaultAsync(p => p.UserCardId == userCardId, ct);

    /// <summary>
    /// AMAÇ: Mevcut kaydı getirir, yoksa SM-2 başlangıç değerleriyle oluşturur.
    /// NEDEN: Kişisel kart ilk kez çalışılırken otomatik progress kaydı oluşturulmalı.
    /// NASIL: GetByUserCardAsync → null ise AddAsync ile yeni kayıt; "upsert" deseni.
    /// </summary>
    public async Task<UserCardProgress> GetOrCreateAsync(int userId, int userCardId, CancellationToken ct = default)
    {
        var existing = await _db.UserCardProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.UserCardId == userCardId, ct);

        if (existing is not null)
            return existing;

        // İlk karşılaşma — SM-2 başlangıç değerleriyle kayıt oluştur
        var progress = new UserCardProgress
        {
            UserId       = userId,
            UserCardId   = userCardId,
            CurrentLevel = 0,
            NextReviewAt = DateTime.UtcNow,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        await _db.UserCardProgresses.AddAsync(progress, ct);
        await _db.SaveChangesAsync(ct);
        return progress;
    }

    /// <summary>
    /// AMAÇ: Bugün tekrarı gelen kişisel kartları getirir — günlük SRS oturumu için.
    /// NEDEN: Kullanıcının kişisel kartları sistem kelimeleriyle ayrı oturumda çalışılabilir.
    /// NASIL: NextReviewAt &lt;= şimdi; UserCard navigation property dahil edilir.
    /// </summary>
    public async Task<IEnumerable<UserCardProgress>> GetDueForReviewAsync(
        int userId,
        int count,
        CancellationToken ct = default)
        => await _db.UserCardProgresses
            .Include(p => p.UserCard)
            .Where(p => p.UserId == userId && p.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(p => p.NextReviewAt)
            .Take(count)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: İlerleme kaydını günceller — SM-2 cevap işlemesinde.
    /// NEDEN: UpdatedAt manuel güncellenir (UserCardProgress BaseEntity'den miras almıyor).
    /// </summary>
    public async Task UpdateAsync(UserCardProgress progress, CancellationToken ct = default)
    {
        progress.UpdatedAt = DateTime.UtcNow;
        _db.UserCardProgresses.Update(progress);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Yeni ilerleme kaydı oluşturur.
    /// </summary>
    public async Task<UserCardProgress> AddAsync(UserCardProgress progress, CancellationToken ct = default)
    {
        await _db.UserCardProgresses.AddAsync(progress, ct);
        await _db.SaveChangesAsync(ct);
        return progress;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
