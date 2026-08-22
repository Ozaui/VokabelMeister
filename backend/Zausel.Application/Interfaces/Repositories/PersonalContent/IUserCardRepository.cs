using Zausel.Application.DTOs;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Interfaces.Repositories.PersonalContent;

public interface IUserCardRepository
{
    Task<PagedResult<UserCardAggregate>> GetPagedForUserAsync(
        int userId, int? categoryId, int? userCategoryId, string? search,
        int page, int pageSize, CancellationToken cancellationToken = default);

    // Sahiplik filtresi burada gömülü — başkasının kartı null döner, Handler'da EntityNotFoundException'a çevrilir.
    Task<UserCardAggregate?> GetByIdForUserAsync(int userCardId, int userId, CancellationToken cancellationToken = default);

    // Duplikat kontrolü (409+force) için — AYNI kullanıcının FrontText'i (case-insensitive, DB
    // collation'a güvenilir) eşleşen BAŞKA bir kartı var mı.
    Task<UserCard?> FindByUserAndFrontTextAsync(int userId, string frontText, int? excludeUserCardId, CancellationToken cancellationToken = default);

    Task AddAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(UserCard userCard, int userId, CancellationToken cancellationToken = default);

    // Create/UpdateUserCardCommand'ın categoryIds[]/userCategoryIds[]/examples[] alanları için —
    // WordConceptRepository.ReplaceWordCategoriesAsync/ReplaceExamplesAsync ile AYNI "tam değişim"
    // deseni: mevcut TÜM bağ/örnek satırları (hard) silinir, verilenlerle değiştirilir. UserCardCategory/
    // UserCardUserCategory/UserCardExample'ın ÜÇÜ de BaseEntity DEĞİL (soft delete yok), bu yüzden
    // WordExample'ın AKSİNE (soft-delete) burada da HARD silme kullanılır.
    Task ReplaceCategoriesAsync(int userCardId, List<int> categoryIds, CancellationToken cancellationToken = default);
    Task ReplaceUserCategoriesAsync(int userCardId, List<int> userCategoryIds, CancellationToken cancellationToken = default);
    Task ReplaceExamplesAsync(int userCardId, List<UserCardExample> newExamples, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
