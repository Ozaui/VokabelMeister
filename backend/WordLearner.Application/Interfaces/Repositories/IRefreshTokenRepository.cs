// ─────────────────────────────────────────────────────────────────────────────
// IRefreshTokenRepository.cs
//
// AMAÇ: RefreshToken'a özel sorguları (hash ile arama, family iptali) IRepository<T>'nin
//       genel CRUD'una ekler.
// NEDEN: Token Family Pattern (REFERENCE/SECURITY.md §1) — refresh/replay tespiti ve
//        "tüm cihazlardan çıkış" (şifre sıfırlama, hesap silme) bu metotları gerektirir.
// BAĞIMLILIKLAR: IRepository<T>, WordLearner.Domain.Entities.RefreshToken.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    // AMAÇ: SHA-256 hash'ine göre refresh token kaydını bulur.
    // NEDEN: İstemciden gelen ham token asla saklanmadığı için, doğrulama her zaman
    //        hash üzerinden yapılır (IPasswordService.HashToken).
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    // AMAÇ: Aynı TokenFamily'e ait tüm refresh token'ları iptal eder (RevokedAt=UtcNow).
    // NEDEN: Replay tespit edildiğinde (kullanılmış bir token tekrar geldiğinde) o
    //        family'nin tamamı güvenli kabul edilmez, hepsi iptal edilir.
    Task RevokeFamilyAsync(string tokenFamily, CancellationToken ct = default);

    // AMAÇ: Bir kullanıcının TÜM refresh token'larını iptal eder (family'den bağımsız).
    // NEDEN: Şifre sıfırlama ve hesap silme sonrası "tüm cihazlardan çıkış" gereksinimi
    //        (REFERENCE/SECURITY.md §7) — o kullanıcıya ait her token, hangi family'den
    //        olursa olsun geçersiz kılınır.
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}
