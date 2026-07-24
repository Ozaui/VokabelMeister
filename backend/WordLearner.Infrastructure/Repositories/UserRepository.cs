// ─────────────────────────────────────────────────────────────────────────────
// UserRepository.cs
//
// AMAÇ: IUserRepository'nin EF Core implementasyonu.
// NEDEN: Repository<T>'yi miras alarak genel CRUD'u yeniden yazmadan yalnızca
//        User'a özgü sorguları ekler.
// BAĞIMLILIKLAR: EF Core, Repository<T>, WordLearnerDbContext, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(WordLearnerDbContext db)
        : base(db) { }

    // AMAÇ: Admin panel kullanıcı listesi — arama Email/FirstName/LastName'de,
    //       `_set` zaten Repository<T>'nin soft-delete global filtresini taşır.
    public async Task<PagedResult<User>> GetPagedAsync(
        string? search,
        string? role,
        int page,
        int pageSize,
        CancellationToken ct = default
    )
    {
        var query = _set.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Email.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search)
            );
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<User>(items, totalCount, page, pageSize);
    }

    // AMAÇ: Admin panel istatistik kartları — toplam/aktif/dondurulmuş kullanıcı sayısı.
    public async Task<(int TotalUsers, int ActiveUsers, int FrozenUsers)> GetStatisticsAsync(
        CancellationToken ct = default
    )
    {
        var total = await _set.CountAsync(ct);
        var active = await _set.CountAsync(u => u.IsActive, ct);
        return (total, active, total - active);
    }

    // AMAÇ: Admin panel kayıt grafiği — `fromUtc`'den bu yana her kaydın CreatedAt'i,
    //       ham liste olarak (günlere gruplama Handler'da yapılır).
    public async Task<IReadOnlyList<DateTime>> GetRegistrationDatesAsync(
        DateTime fromUtc,
        CancellationToken ct = default
    ) => await _set.Where(u => u.CreatedAt >= fromUtc).Select(u => u.CreatedAt).ToListAsync(ct);

    // AMAÇ: E-postaya göre kullanıcı bulur, soft delete filtresini bilerek yok sayar.
    // NEDEN: IgnoreQueryFilters() olmadan grace period içindeki (soft-delete'li) bir
    //        hesap login/register akışlarında hiç görünmez — hesap kurtarma ve
    //        "e-posta zaten kullanımda" kontrolü bu satır olmadan çalışamaz.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, ct);

    // AMAÇ: Id'ye göre kullanıcı bulur, soft delete filtresini bilerek yok sayar.
    // NEDEN: bkz. IUserRepository — GetByEmailAsync ile aynı gerekçe, Id ile arama.
    public Task<User?> GetByIdIncludingDeletedAsync(int id, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct);

    // AMAÇ: Google Sign-In sub (GoogleId) değerine göre kullanıcı bulur.
    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    // AMAÇ: Apple Sign-In sub (AppleId) değerine göre kullanıcı bulur.
    public Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.AppleId == appleId, ct);

    // AMAÇ: Anonimleştirilmiş bir hesabın OriginalEmailHash'i verilen hash ile eşleşiyor mu bakar.
    public Task<bool> OriginalEmailHashExistsAsync(
        string emailHash,
        CancellationToken ct = default
    ) => _set.IgnoreQueryFilters().AnyAsync(u => u.OriginalEmailHash == emailHash, ct);
}
