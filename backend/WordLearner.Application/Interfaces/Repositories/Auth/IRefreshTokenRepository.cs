using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    // Şifre sıfırlama/hesap silme onayı sonrası "tüm cihazlardan çıkış" (SECURITY.md §7/§9).
    Task RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default);

    // Token Family Pattern: bir refresh token replay edilirse (zaten kullanılmış tekrar gelirse)
    // aynı family'deki TÜM token'lar iptal edilir (SECURITY.md §1).
    Task RevokeFamilyAsync(string tokenFamily, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
