/// <summary>
/// FriendshipRepository.cs
///
/// AMAÇ: Arkadaşlık sorguları — çift yönlü FK nedeniyle OR koşullu sorgular gerekir.
/// NEDEN: Kullanıcı hem Requester hem Receiver olabilir; tek yönlü sorgu yetmez.
/// BAĞIMLILIKLAR: IFriendshipRepository (Application), WordLearnerDbContext, Friendship entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Social;

/// <summary>
/// Arkadaşlık repository implementasyonu.
///
/// AMAÇ: IFriendshipRepository sözleşmesini karşılamak.
/// NEDEN: Friendship BaseEntity'den miras almaz — generic Repository kullanılamaz.
/// </summary>
public class FriendshipRepository : IFriendshipRepository
{
    private readonly WordLearnerDbContext _db;

    public FriendshipRepository(WordLearnerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// AMAÇ: Kullanıcının kabul edilmiş tüm arkadaşlarını getirir.
    /// NEDEN: Arkadaş listesi — kullanıcı hem gönderen hem alıcı olabilir.
    /// NASIL: OR koşuluyla her iki pozisyonda da arama; Requester ve Receiver navigation yüklenir.
    /// </summary>
    public async Task<IEnumerable<Friendship>> GetFriendsAsync(int userId, CancellationToken ct = default)
        => await _db.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f =>
                (f.RequesterId == userId || f.ReceiverId == userId) &&
                f.Status == "Accepted")
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Kullanıcıya gelen bekleyen istekleri getirir.
    /// NEDEN: "Gelen istekler" sekmesi — sadece alıcı pozisyonunda pending istekler.
    /// NASIL: ReceiverId=userId AND Status='Pending'; Requester bilgisi yüklenir (kim gönderdi).
    /// </summary>
    public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(int userId, CancellationToken ct = default)
        => await _db.Friendships
            .Include(f => f.Requester)
            .Where(f => f.ReceiverId == userId && f.Status == "Pending")
            .OrderByDescending(f => f.RequestedAt)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: İki kullanıcı arasındaki mevcut arkadaşlık kaydını bulur.
    /// NEDEN: İstek göndermeden önce mevcut ilişki kontrol edilir — çift istek veya engel kontrolü.
    /// NASIL: Her iki yön de kontrol edilir: (A→B) veya (B→A).
    /// </summary>
    public Task<Friendship?> GetByUsersAsync(int userId1, int userId2, CancellationToken ct = default)
        => _db.Friendships.FirstOrDefaultAsync(f =>
            (f.RequesterId == userId1 && f.ReceiverId == userId2) ||
            (f.RequesterId == userId2 && f.ReceiverId == userId1), ct);

    /// <summary>
    /// AMAÇ: Yeni arkadaşlık isteği kaydı oluşturur.
    /// </summary>
    public async Task<Friendship> AddAsync(Friendship friendship, CancellationToken ct = default)
    {
        await _db.Friendships.AddAsync(friendship, ct);
        await _db.SaveChangesAsync(ct);
        return friendship;
    }

    /// <summary>
    /// AMAÇ: Arkadaşlık durumunu günceller — Accepted, Rejected veya Blocked.
    /// NEDEN: Status değişikliği ve UpdatedAt güncellenmesi gerekir.
    /// </summary>
    public async Task UpdateAsync(Friendship friendship, CancellationToken ct = default)
    {
        friendship.UpdatedAt = DateTime.UtcNow;
        _db.Friendships.Update(friendship);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Arkadaşlık kaydını fiziksel olarak siler — arkadaşlıktan çıkma.
    /// NEDEN: Friendship entity'de soft delete yoktur; ilişki tamamen kaldırılır.
    /// </summary>
    public async Task DeleteAsync(Friendship friendship, CancellationToken ct = default)
    {
        _db.Friendships.Remove(friendship);
        await _db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
