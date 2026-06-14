/// <summary>
/// ISharedContentRepository.cs
///
/// AMAÇ: Paylaşım linki sorgularını tanımlar — token ile önizleme ve sahip listeleme.
/// NEDEN: ShareToken ile arama ve ViewCount güncelleme generic CRUD'un ötesindedir.
/// BAĞIMLILIKLAR: SharedContent entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Paylaşım linki repository arayüzü.
///
/// AMAÇ: UUID token ile içerik önizleme ve paylaşım yönetimi sorgularını tanımlamak.
/// NEDEN: Giriş yapmadan token araması ve ViewCount artırma özel operasyonlar gerektirir.
/// </summary>
public interface ISharedContentRepository
{
    /// <summary>
    /// AMAÇ: UUID token ile paylaşım kaydını getirir — giriş yapmadan önizleme için.
    /// NEDEN: Paylaşım önizleme sayfası auth gerektirmez; token geçerliyse içerik gösterilir.
    /// NASIL: IsActive=true ve ExpiresAt null veya &gt; şimdi filtresi uygulanır.
    /// </summary>
    Task<SharedContent?> GetByTokenAsync(string shareToken, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bir kullanıcının oluşturduğu tüm aktif paylaşım linklerini getirir.
    /// NEDEN: "Paylaşımlarım" listesi için — kullanıcı linklerini yönetebilmeli.
    /// </summary>
    Task<IEnumerable<SharedContent>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni paylaşım linki kaydı oluşturur.
    /// </summary>
    Task<SharedContent> AddAsync(SharedContent content, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Paylaşım kaydını günceller — IsActive değiştirme veya ViewCount artırma için.
    /// </summary>
    Task UpdateAsync(SharedContent content, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Görüntülenme sayacını bir artırır.
    /// NEDEN: Her önizleme isteğinde ViewCount güncellenir; ayrı metod olması performans açısından önemli.
    /// NASIL: Tek alan güncellemesi — tüm entity yüklenmez.
    /// </summary>
    Task IncrementViewCountAsync(int id, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
