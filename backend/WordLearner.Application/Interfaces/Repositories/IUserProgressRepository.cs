/// <summary>
/// IUserProgressRepository.cs
///
/// AMAÇ: Sistem kelimesi SRS ilerleme sorgularını tanımlar.
/// NEDEN: SM-2 algoritması için tekrar tarihi, yeni kelime ve istatistik sorguları gerekir.
/// BAĞIMLILIKLAR: UserProgress entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Sistem kelimesi SRS ilerleme repository arayüzü.
///
/// AMAÇ: Kullanıcının sistem kelimesi öğrenme durumunu sorgulamak ve güncellemek.
/// NEDEN: SM-2 oturum oluşturma için özel sorgular generic CRUD'la karşılanamaz.
/// </summary>
public interface IUserProgressRepository
{
    /// <summary>
    /// AMAÇ: Kullanıcı + kelime çifti için mevcut ilerleme kaydını getirir.
    /// NEDEN: Cevap işlenirken mevcut SM-2 durumu okunur.
    /// </summary>
    Task<UserProgress?> GetByUserAndWordAsync(int userId, int wordId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bugün tekrarı gelen kelimelerin ilerleme kayıtlarını getirir.
    /// NEDEN: Günlük SRS oturumu bu listeyi kullanarak kartları sıraya dizer.
    /// NASIL: NextReviewAt &lt;= DateTime.UtcNow.Date filtresi; count kadar kayıt döner.
    /// </summary>
    Task<IEnumerable<UserProgress>> GetDueForReviewAsync(
        int userId,
        int count,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının belirli bir kelimedeki ilerleme kaydını günceller.
    /// NEDEN: SM-2 cevap işlemede CurrentLevel, EasinessFactor, NextReviewAt güncellenir.
    /// </summary>
    Task UpdateAsync(UserProgress progress, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni ilerleme kaydı oluşturur — kullanıcı kelimeyle ilk kez karşılaştığında.
    /// NEDEN: UserProgress kaydı yoksa SM-2 başlangıç değerleriyle oluşturulur.
    /// </summary>
    Task<UserProgress> AddAsync(UserProgress progress, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının genel öğrenme istatistiklerini getirir.
    /// NEDEN: Profil/istatistik ekranı için özet bilgi (toplam öğrenilen, mastery ortalaması vb.).
    /// NASIL: Gruplama ve agregasyon sorgusu döner.
    /// </summary>
    Task<(int TotalLearned, int Mastered, double AverageSuccessRate)> GetStatisticsAsync(
        int userId,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
