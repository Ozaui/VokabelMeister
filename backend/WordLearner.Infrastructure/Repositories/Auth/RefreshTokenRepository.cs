/// <summary>
/// RefreshTokenRepository.cs
///
/// AMAÇ: Refresh token CRUD ve Token Family Pattern operasyonlarının implementasyonu.
/// NEDEN: IRefreshTokenRepository sözleşmesi IRepository'den türemez — RefreshToken BaseEntity değil.
/// BAĞIMLILIKLAR: IRefreshTokenRepository (Application), WordLearnerDbContext, RefreshToken entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Auth;

/// <summary>
/// Refresh token repository implementasyonu.
///
/// AMAÇ: JWT yenileme ve güvenlik iptal operasyonlarını sağlamak.
/// NEDEN: Token Family Pattern ve toplu iptal işlemleri özel SQL gerektiriyor.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly WordLearnerDbContext _db;

    public RefreshTokenRepository(WordLearnerDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// AMAÇ: Token hash'i ile aktif, süresi dolmamış token kaydını bulur.
    /// NEDEN: Refresh akışında gelen token hash'lenerek bu sorguyla doğrulanır.
    /// NASIL: IsUsed=false, RevokedAt=null ve ExpiresAt>şimdi koşulları birlikte aranır.
    /// </summary>
    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) =>
        _db.RefreshTokens.FirstOrDefaultAsync(
            t =>
                t.TokenHash == tokenHash
                && !t.IsUsed
                && t.RevokedAt == null
                && t.ExpiresAt > DateTime.UtcNow,
            ct
        );

    /// <summary>
    /// AMAÇ: Bir aileye ait tüm token kayıtlarını getirir.
    /// NEDEN: Token Family Pattern — eski token kullanıldığında tüm aile iptal edilecek.
    /// NASIL: TokenFamily eşleşmesi; IsUsed veya RevokedAt durumuna bakılmaz (hepsi gözden geçirilir).
    /// </summary>
    public async Task<IEnumerable<RefreshToken>> GetByFamilyAsync(
        string tokenFamily,
        CancellationToken ct = default
    ) => await _db.RefreshTokens.Where(t => t.TokenFamily == tokenFamily).ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Kullanıcının tüm aktif token'larını iptal eder — çıkış veya şifre değişikliği için.
    /// NEDEN: "Tüm cihazlardan çıkış" güvenlik özelliği için toplu iptal gerekir.
    /// NASIL: ExecuteUpdateAsync ile tek SQL UPDATE — teker teker entity yüklemek yerine.
    /// </summary>
    public async Task RevokeAllByUserAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await _db
            .RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>
    /// AMAÇ: Bir token ailesinin tamamını iptal eder — replay attack tespit edildiğinde.
    /// NEDEN: Çalınmış token kullanıldığında aynı ailedeki geçerli token anında geçersizleşmeli.
    /// NASIL: ExecuteUpdateAsync ile toplu UPDATE; tüm aile tek sorguda güncellenir.
    /// </summary>
    public async Task RevokeAllByFamilyAsync(string tokenFamily, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await _db
            .RefreshTokens.Where(t => t.TokenFamily == tokenFamily && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>
    /// AMAÇ: Yeni token kaydını ekler ve kaydeder.
    /// </summary>
    public async Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _db.RefreshTokens.AddAsync(token, ct);
        await _db.SaveChangesAsync(ct);
        return token;
    }

    /// <summary>
    /// AMAÇ: Token kaydını günceller — IsUsed=true yapma veya RevokedAt set etme.
    /// </summary>
    public async Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        _db.RefreshTokens.Update(token);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// AMAÇ: Süresi dolmuş ve kullanılmış token kayıtlarını fiziksel olarak siler.
    /// NEDEN: RefreshTokens tablosu şişmemeli — periyodik temizlik arka planda çalışır.
    /// NASIL: ExecuteDeleteAsync ile toplu DELETE — tek SQL sorgusu.
    /// </summary>
    public async Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        await _db
            .RefreshTokens.Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
            .ExecuteDeleteAsync(ct);
    }
}
