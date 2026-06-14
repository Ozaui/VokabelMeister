/// <summary>
/// IFriendshipRepository.cs
///
/// AMAÇ: Arkadaşlık ilişkisi sorgularını tanımlar — çift yönlü FK nedeniyle özel sorgular gerekir.
/// NEDEN: Friendship entity'sinde RequesterId ve ReceiverId iki ayrı FK; UNION sorgusu gerekir.
/// BAĞIMLILIKLAR: Friendship entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Arkadaşlık repository arayüzü.
///
/// AMAÇ: İki yönlü arkadaşlık sorgularını ve durum güncellemelerini tanımlamak.
/// NEDEN: Requester veya Receiver olarak kullanıcıyı bulmak için OR koşulu gerekli.
/// </summary>
public interface IFriendshipRepository
{
    /// <summary>
    /// AMAÇ: Kullanıcının kabul edilmiş tüm arkadaşlarını getirir.
    /// NEDEN: Arkadaş listesi ekranı için — hem gönderen hem alıcı pozisyonunda olabilir.
    /// NASIL: (RequesterId=userId OR ReceiverId=userId) AND Status='Accepted' sorgusu.
    /// </summary>
    Task<IEnumerable<Friendship>> GetFriendsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcıya gelen bekleyen arkadaşlık isteklerini getirir.
    /// NEDEN: "İstekler" sekmesi için — sadece alıcı pozisyonundaki pending istekler.
    /// NASIL: ReceiverId=userId AND Status='Pending' sorgusu.
    /// </summary>
    Task<IEnumerable<Friendship>> GetPendingRequestsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: İki kullanıcı arasındaki arkadaşlık kaydını bulur.
    /// NEDEN: İstek göndermeden önce mevcut ilişki kontrol edilir (çift istek engellenir).
    /// NASIL: (RequesterId=u1 AND ReceiverId=u2) OR (RequesterId=u2 AND ReceiverId=u1) sorgusu.
    /// </summary>
    Task<Friendship?> GetByUsersAsync(int userId1, int userId2, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni arkadaşlık isteği oluşturur.
    /// </summary>
    Task<Friendship> AddAsync(Friendship friendship, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Arkadaşlık durumunu günceller — kabul, red veya engelleme için.
    /// NEDEN: Status alanı Pending → Accepted/Rejected/Blocked olarak değiştirilir.
    /// </summary>
    Task UpdateAsync(Friendship friendship, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Arkadaşlık kaydını fiziksel olarak siler — arkadaşlıktan çıkma.
    /// NEDEN: Friendship entity BaseEntity'den miras almıyor — soft delete yoktur.
    /// </summary>
    Task DeleteAsync(Friendship friendship, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
