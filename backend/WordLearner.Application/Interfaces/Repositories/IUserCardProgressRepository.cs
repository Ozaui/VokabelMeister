/// <summary>
/// IUserCardProgressRepository.cs
///
/// AMAÇ: Kişisel kart SRS ilerleme sorgularını tanımlar.
/// NEDEN: UserProgress ile aynı mantık fakat UserCard entity'si için.
/// BAĞIMLILIKLAR: UserCardProgress entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Kişisel kart SRS ilerleme repository arayüzü.
///
/// AMAÇ: Kullanıcının kişisel kart öğrenme durumunu sorgulamak ve güncellemek.
/// NEDEN: Kişisel kartlar ayrı SRS tablosunda tutulur — sistem kelimeleriyle karışmaz.
/// </summary>
public interface IUserCardProgressRepository
{
    /// <summary>
    /// AMAÇ: Belirli bir kart için ilerleme kaydını getirir.
    /// NEDEN: Cevap işlenirken mevcut SM-2 durumu okunur.
    /// </summary>
    Task<UserCardProgress?> GetByUserCardAsync(int userCardId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Mevcut ilerleme kaydını getirir, yoksa varsayılan değerlerle oluşturur.
    /// NEDEN: Kişisel kartlar için ilerleme kaydı otomatik başlatılmalı — ilk görüşte oluşur.
    /// NASIL: GetByUserCardAsync null dönerse AddAsync ile yeni kayıt oluşturulur.
    /// </summary>
    Task<UserCardProgress> GetOrCreateAsync(int userId, int userCardId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bugün tekrarı gelen kişisel kartların ilerleme kayıtlarını getirir.
    /// NEDEN: Günlük SRS oturumu kişisel kart tekrarlarını içerebilir.
    /// NASIL: NextReviewAt &lt;= DateTime.UtcNow filtresi; kullanıcıya ait kartlar.
    /// </summary>
    Task<IEnumerable<UserCardProgress>> GetDueForReviewAsync(
        int userId,
        int count,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Mevcut ilerleme kaydını günceller — SM-2 cevap işlemesinde.
    /// </summary>
    Task UpdateAsync(UserCardProgress progress, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni ilerleme kaydı oluşturur.
    /// </summary>
    Task<UserCardProgress> AddAsync(UserCardProgress progress, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
