// ─────────────────────────────────────────────────────────────────────────────
// UserRepository.cs
//
// AMAÇ: IUserRepository'nin EF Core implementasyonu.
// NEDEN: Repository<T>'yi miras alarak genel CRUD'u yeniden yazmadan yalnızca
//        User'a özgü sorguları ekler.
// BAĞIMLILIKLAR: EF Core, Repository<T>, WordLearnerDbContext, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(WordLearnerDbContext db)
        : base(db) { }

    // AMAÇ: E-postaya göre kullanıcı bulur, soft delete filtresini bilerek yok sayar.
    // NEDEN: IgnoreQueryFilters() olmadan grace period içindeki (soft-delete'li) bir
    //        hesap login/register akışlarında hiç görünmez — hesap kurtarma ve
    //        "e-posta zaten kullanımda" kontrolü bu satır olmadan çalışamaz.
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, ct);

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
