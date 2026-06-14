/// <summary>
/// UserRepository.cs
///
/// AMAÇ: Kullanıcı entity'si için Repository&lt;User&gt; implementasyonu.
/// NEDEN: E-posta, Google ID ve Apple ID ile arama generic CRUD'a eklenmeli.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), IUserRepository (Application), WordLearnerDbContext
/// </summary>
using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Auth;

/// <summary>
/// Kullanıcı repository implementasyonu.
///
/// AMAÇ: IUserRepository sözleşmesini karşılamak.
/// NEDEN: Auth akışına özgü sorgular (e-posta, Google ID, Apple ID) burada yazılır.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(WordLearnerDbContext db)
        : base(db) { }

    /// <summary>
    /// AMAÇ: E-posta ile kullanıcı bulur — login akışının ilk adımı.
    /// NEDEN: Giriş formunda e-posta girilir; şifre karşılaştırması için User nesnesi lazım.
    /// NASIL: ToLower normalizasyonu — büyük/küçük harf farkı gözetilmez.
    /// </summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _set.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);

    /// <summary>
    /// AMAÇ: Google ID ile kullanıcı bulur — Google OAuth akışı.
    /// NEDEN: Google token doğrulandıktan sonra sistemdeki eşleşen hesap aranır.
    /// NASIL: GoogleId nullable — null kayıtlar filtreden geçmez.
    /// </summary>
    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default) =>
        _set.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    /// <summary>
    /// AMAÇ: Apple ID ile kullanıcı bulur — Apple Sign In akışı.
    /// NEDEN: Apple token doğrulandıktan sonra sistemdeki eşleşen hesap aranır.
    /// </summary>
    public Task<User?> GetByAppleIdAsync(string appleId, CancellationToken ct = default) =>
        _set.FirstOrDefaultAsync(u => u.AppleId == appleId, ct);

    /// <summary>
    /// AMAÇ: E-posta kayıtlı mı kontrol eder — kayıt sırasında duplikasyon önleme.
    /// NEDEN: AnyAsync ile COUNT yerine EXISTS sorgusu üretilir — daha performanslı.
    /// NASIL: ToLower normalizasyonu uygulanır.
    /// </summary>
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        _set.AnyAsync(u => u.Email.ToLower() == email.ToLower(), ct);
}
