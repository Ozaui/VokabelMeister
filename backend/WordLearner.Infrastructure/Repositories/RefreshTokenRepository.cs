// ─────────────────────────────────────────────────────────────────────────────
// RefreshTokenRepository.cs
//
// AMAÇ: IRefreshTokenRepository'nin EF Core implementasyonu.
// NEDEN: Repository<T>'yi miras alarak genel CRUD'u yeniden yazmadan yalnızca
//        RefreshToken'a özgü sorguları (hash arama, family/kullanıcı iptali) ekler.
// BAĞIMLILIKLAR: EF Core, Repository<T>, WordLearnerDbContext, RefreshToken entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(WordLearnerDbContext db)
        : base(db) { }

    // AMAÇ: SHA-256 hash'ine göre refresh token kaydını bulur.
    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) => _set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    // AMAÇ: Aynı TokenFamily'e ait, henüz iptal edilmemiş tüm token'ları RevokedAt=UtcNow yapar.
    // NEDEN: Replay tespitinde tüm family güvensiz sayılır — tek bir token değil, hepsi iptal edilir.
    public async Task RevokeFamilyAsync(string tokenFamily, CancellationToken ct = default)
    {
        var tokens = await _set.Where(t => t.TokenFamily == tokenFamily && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    // AMAÇ: Bir kullanıcının, hangi family'den olursa olsun, henüz iptal edilmemiş
    //       tüm refresh token'larını RevokedAt=UtcNow yapar.
    // NEDEN: Şifre sıfırlama/hesap silme sonrası "tüm cihazlardan çıkış" gereksinimi.
    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var tokens = await _set.Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
