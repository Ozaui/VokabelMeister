/// <summary>
/// UserCardRepository.cs
///
/// AMAÇ: Kişisel kart sorgularının implementasyonu — güvenlik kritik, UserId filtresi zorunlu.
/// NEDEN: Her sorgu UserId ile kısıtlanmazsa kullanıcı başkasının kartına erişebilir.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IUserCardRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.UserContent;

/// <summary>
/// Kişisel kart repository implementasyonu.
///
/// AMAÇ: IUserCardRepository sözleşmesini karşılamak.
/// GÜVENLİK: Her sorguda UserId filtresi zorunludur — entity'nin kendi yorumu açıkça belirtmiş.
/// </summary>
public class UserCardRepository : Repository<UserCard>, IUserCardRepository
{
    public UserCardRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Kartı sahiplik kontrolüyle getirir — detay, güncelleme ve silme öncesi.
    /// NEDEN: Başka kullanıcının kartına erişim engellenmeli; null dönüşü controller'da 404 verir.
    /// GÜVENLİK: 404 vermek 403'ten daha güvenli — kartın varlığı bile ifşa edilmez.
    /// </summary>
    public Task<UserCard?> GetByUserAndIdAsync(int userId, int cardId, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId, ct);

    /// <summary>
    /// AMAÇ: Kullanıcının kartlarını kategori filtresiyle sayfalı getirir.
    /// NEDEN: Kart listesi ekranı filtreli ve sayfalı görünüm gerektirir.
    /// NASIL: NULL filtre = uygulanmaz; TotalCount sayfalama için ayrıca hesaplanır.
    /// </summary>
    public async Task<(IEnumerable<UserCard> Cards, int TotalCount)> GetPagedByUserAsync(
        int userId,
        int? categoryId,
        int? userCategoryId,
        int page,
        int size,
        CancellationToken ct = default)
    {
        // UserId filtresi her zaman ilk koşul — güvenlik garantisi
        var query = _set.Where(c => c.UserId == userId);

        if (categoryId.HasValue)
            query = query.Where(c =>
                c.UserCardCategories.Any(cc => cc.CategoryId == categoryId.Value));

        if (userCategoryId.HasValue)
            query = query.Where(c =>
                c.UserCardUserCategories.Any(uc => uc.UserCategoryId == userCategoryId.Value));

        var totalCount = await query.CountAsync(ct);

        var cards = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (cards, totalCount);
    }

    /// <summary>
    /// AMAÇ: Kartı tüm ilişkili verilerle getirir — düzenleme ekranı için.
    /// NEDEN: Düzenleme formunda örnekler ve kategoriler de gösterilmeli.
    /// NASIL: UserCardExamples, UserCardCategories ve UserCardUserCategories eager load edilir.
    /// GÜVENLİK: UserId filtresi Include zinciriyle de korunuyor.
    /// </summary>
    public Task<UserCard?> GetWithDetailsAsync(int userId, int cardId, CancellationToken ct = default)
        => _set
            .Include(c => c.UserCardExamples)
            .Include(c => c.UserCardCategories)
                .ThenInclude(cc => cc.Category)
            .Include(c => c.UserCardUserCategories)
                .ThenInclude(uc => uc.UserCategory)
            .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId, ct);
}
