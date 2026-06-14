/// <summary>
/// IRefreshTokenRepository.cs
///
/// AMAÇ: Refresh token CRUD ve Token Family Pattern sorgularını tanımlar.
/// NEDEN: Güvenlik kritik — replay attack tespiti için aile bazlı iptal gerekir.
/// BAĞIMLILIKLAR: RefreshToken entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Refresh token repository arayüzü.
///
/// AMAÇ: JWT yenileme ve güvenlik iptal operasyonlarını tanımlamak.
/// NEDEN: Token Family Pattern implementasyonu generic CRUD'un ötesinde sorgular gerektirir.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// AMAÇ: Token hash'i ile token kaydını bulur — yenileme akışında doğrulama için.
    /// NEDEN: Gelen raw token hash'lenerek veritabanıyla karşılaştırılır.
    /// NASIL: TokenHash eşleşmeli, IsUsed=false ve RevokedAt=null ve ExpiresAt>şimdi olmalı.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bir token ailesine ait tüm aktif token'ları getirir.
    /// NEDEN: Token Family Pattern — aynı aile token'larından herhangi biri yeniden kullanılırsa
    ///        tüm aile iptal edilir (replay attack tespiti).
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetByFamilyAsync(string tokenFamily, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bir kullanıcının tüm aktif token'larını iptal eder — çıkış veya şifre değişikliğinde.
    /// NEDEN: Tüm cihazlardan çıkış güvenliği için tek seferde iptal gerekir.
    /// NASIL: RevokedAt=şimdi yapılır, IsUsed değişmez.
    /// </summary>
    Task RevokeAllByUserAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Bir token ailesinin tamamını iptal eder — replay attack tespit edildiğinde.
    /// NEDEN: Çalınmış token kullanıldığında aynı ailedeki geçerli token da hemen geçersizleşmeli.
    /// NASIL: RevokedAt=şimdi ile tüm aile kayıtları güncellenir.
    /// </summary>
    Task RevokeAllByFamilyAsync(string tokenFamily, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Yeni token kaydını ekler.
    /// NEDEN: Login ve refresh akışında her seferinde yeni token kaydı yazılır.
    /// </summary>
    Task<RefreshToken> AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Mevcut token kaydını günceller — IsUsed veya RevokedAt alanları için.
    /// </summary>
    Task UpdateAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Süresi dolmuş ve kullanılmış eski token kayıtlarını temizler.
    /// NEDEN: Tablo şişmemesi için periyodik temizlik gerekir.
    /// NASIL: ExpiresAt &lt; şimdi VEYA IsUsed=true olanlar silinir.
    /// </summary>
    Task CleanupExpiredAsync(CancellationToken ct = default);
}
