/// <summary>
/// UserCategoryRepository.cs
///
/// AMAÇ: Kişisel kategori sorgularının implementasyonu — güvenlik: her sorgu UserId filtreli.
/// NEDEN: Kullanıcı başkasının kategorisine erişememeli; tüm sorgularda UserId zorunlu.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IUserCategoryRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.UserContent;

/// <summary>
/// Kişisel kategori repository implementasyonu.
///
/// AMAÇ: IUserCategoryRepository sözleşmesini karşılamak.
/// GÜVENLİK: Tüm sorgularda UserId filtresi zorunlu — başkasının kategorisi görülemez.
/// </summary>
public class UserCategoryRepository : Repository<UserCategory>, IUserCategoryRepository
{
    public UserCategoryRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Kullanıcının tüm kişisel kategorilerini getirir.
    /// NEDEN: Kişisel kategori listesi ve kart oluşturma formu için.
    /// GÜVENLİK: UserId filtresi — başkasının kategorisi sorguya girmez.
    /// </summary>
    public async Task<IEnumerable<UserCategory>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await _set
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Kategoriyi sahiplik kontrolüyle getirir — güncelleme ve silme öncesi.
    /// NEDEN: Başka kullanıcının kategorisi güncellenememeli; null dönüşü controller'da 404 verir.
    /// NASIL: Id + UserId filtresi — her ikisi de eşleşmezse null döner.
    /// </summary>
    public Task<UserCategory?> GetByUserAndIdAsync(int userId, int categoryId, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, ct);

    /// <summary>
    /// AMAÇ: Aynı kullanıcıda aynı isimde kategori var mı kontrol eder.
    /// NEDEN: Duplikasyon önleme — kullanıcı aynı isimde iki kategori oluşturamamalı.
    /// NASIL: AnyAsync ile EXISTS sorgusu; ToLower normalizasyonu büyük/küçük harf farkını giderir.
    /// </summary>
    public Task<bool> ExistsByNameAsync(int userId, string name, CancellationToken ct = default)
        => _set.AnyAsync(c =>
            c.UserId == userId &&
            c.Name.ToLower() == name.ToLower(), ct);
}
