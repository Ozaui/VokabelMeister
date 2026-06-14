/// <summary>
/// IUserCategoryRepository.cs
///
/// AMAÇ: Kişisel kategori sorgularını tanımlar — sadece sahibine erişim zorunludur.
/// NEDEN: UserId filtresi güvenlik gereği her sorguda bulunmak zorunda.
/// BAĞIMLILIKLAR: IRepository (generic), UserCategory entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Kişisel kategori repository arayüzü.
///
/// AMAÇ: Kullanıcının kişisel kategorilerini güvenli biçimde sorgulamak.
/// GÜVENLİK: UserId parametresi olmayan hiçbir sorgu yazılmamalı.
/// </summary>
public interface IUserCategoryRepository : IRepository<UserCategory>
{
    /// <summary>
    /// AMAÇ: Kullanıcının tüm kişisel kategorilerini getirir.
    /// NEDEN: Kişisel kategori ekranı ve kart oluşturma formu için.
    /// GÜVENLİK: UserId filtresi zorunlu — başkasının kategorisi görülemez.
    /// </summary>
    Task<IEnumerable<UserCategory>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Belirtilen kategorinin belirtilen kullanıcıya ait olduğunu doğrulayarak getirir.
    /// NEDEN: Güncelleme ve silme operasyonlarında sahiplik kontrolü zorunlu.
    /// NASIL: Id + UserId filtresi ile sorgu; eşleşmezse null döner.
    /// </summary>
    Task<UserCategory?> GetByUserAndIdAsync(int userId, int categoryId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Aynı kullanıcıda aynı isimde kategori var mı kontrol eder.
    /// NEDEN: Kullanıcı aynı isimde iki kategori oluşturamamalı.
    /// </summary>
    Task<bool> ExistsByNameAsync(int userId, string name, CancellationToken ct = default);
}
