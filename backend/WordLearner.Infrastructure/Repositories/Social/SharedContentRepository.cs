/// <summary>
/// SharedContentRepository.cs
///
/// AMAÇ: Paylaşım linki sorgularının implementasyonu — token ile erişim ve görüntüleme sayımı.
/// NEDEN: ShareToken araması ve ViewCount artırma performanslı implementasyon gerektirir.
/// BAĞIMLILIKLAR: ISharedContentRepository (Application), WordLearnerDbContext, SharedContent entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Social;

/// <summary>
/// Paylaşım linki repository implementasyonu.
///
/// AMAÇ: ISharedContentRepository sözleşmesini karşılamak.
/// NEDEN: SharedContent BaseEntity'den miras almaz — generic Repository kullanılamaz.
/// </summary>
public class SharedContentRepository : ISharedContentRepository
{
    private readonly WordLearnerDbContext _db;

    public SharedContentRepository(WordLearnerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// AMAÇ: UUID token ile aktif ve süresi dolmamış paylaşım kaydını getirir.
    /// NEDEN: Paylaşım önizleme sayfası auth gerektirmez — sadece token gerekli.
    /// NASIL: IsActive=true ve (ExpiresAt null VEYA ExpiresAt > şimdi) filtresi.
    /// </summary>
    public Task<SharedContent?> GetByTokenAsync(string shareToken, CancellationToken ct = default)
        => _db.SharedContents
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s =>
                s.ShareToken == shareToken &&
                s.IsActive &&
                (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow), ct);

    /// <summary>
    /// AMAÇ: Kullanıcının oluşturduğu tüm paylaşım linklerini getirir.
    /// NEDEN: "Paylaşımlarım" listesi için kullanıcı kendi linklerini yönetebilmeli.
    /// NASIL: OwnerId filtresi; oluşturulma tarihine göre azalan sıra.
    /// </summary>
    public async Task<IEnumerable<SharedContent>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
        => await _db.SharedContents
            .Where(s => s.OwnerId == ownerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Yeni paylaşım linki kaydı oluşturur.
    /// </summary>
    public async Task<SharedContent> AddAsync(SharedContent content, CancellationToken ct = default)
    {
        await _db.SharedContents.AddAsync(content, ct);
        await _db.SaveChangesAsync(ct);
        return content;
    }

    /// <summary>
    /// AMAÇ: Paylaşım kaydını günceller — IsActive değiştirme veya ExpiresAt ayarlama.
    /// </summary>
    public async Task UpdateAsync(SharedContent content, CancellationToken ct = default)
    {
        _db.SharedContents.Update(content);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Görüntülenme sayacını bir artırır.
    /// NEDEN: Her önizleme isteğinde ViewCount güncellenir — istatistik için.
    /// NASIL: ExecuteUpdateAsync ile tek alan güncellemesi; tüm entity yüklenmez — performanslı.
    /// </summary>
    public async Task IncrementViewCountAsync(int id, CancellationToken ct = default)
    {
        await _db.SharedContents
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(
                x => x.ViewCount,
                x => x.ViewCount + 1), ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
