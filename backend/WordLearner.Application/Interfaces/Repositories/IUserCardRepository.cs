/// <summary>
/// IUserCardRepository.cs
///
/// AMAÇ: Kişisel kart sorgularını tanımlar — güvenlik kritik, UserId filtresi zorunlu.
/// NEDEN: Her sorgu UserId ile kısıtlanmazsa kullanıcı başkasının kartına erişebilir.
/// BAĞIMLILIKLAR: IRepository (generic), UserCard entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Kişisel kart repository arayüzü.
///
/// AMAÇ: Kullanıcının kişisel flash kartlarını güvenli biçimde yönetmek.
/// GÜVENLİK: UserId parametresi olmayan hiçbir sorgu yazılmamalı — entity'nin kendi yorumu var.
/// </summary>
public interface IUserCardRepository : IRepository<UserCard>
{
    /// <summary>
    /// AMAÇ: Belirtilen kartın belirtilen kullanıcıya ait olduğunu doğrulayarak getirir.
    /// NEDEN: Detay, güncelleme ve silme öncesi sahiplik kontrolü zorunlu.
    /// GÜVENLİK: Eşleşmezse null döner — controller 404 verir, 403 vermez (güvenlik).
    /// </summary>
    Task<UserCard?> GetByUserAndIdAsync(int userId, int cardId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının tüm kartlarını kategori ve kişisel kategori filtresiyle sayfalı getirir.
    /// NEDEN: Kart listesi ekranı filtreli ve sayfalı görünüm gerektirir.
    /// GÜVENLİK: UserId parametresi zorunlu.
    /// NASIL: NULL parametre = filtre uygulanmaz.
    /// </summary>
    Task<(IEnumerable<UserCard> Cards, int TotalCount)> GetPagedByUserAsync(
        int userId,
        int? categoryId,
        int? userCategoryId,
        int page,
        int size,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kullanıcının kartını ilişkili tüm verilerle birlikte getirir.
    /// NEDEN: Kart düzenleme ekranı için örnekler ve kategoriler de gerekli.
    /// NASIL: UserCardExamples, UserCardCategories ve UserCardUserCategories eager load edilir.
    /// </summary>
    Task<UserCard?> GetWithDetailsAsync(int userId, int cardId, CancellationToken ct = default);
}
