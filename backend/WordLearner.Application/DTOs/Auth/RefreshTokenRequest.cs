/// <summary>
/// RefreshTokenRequest.cs
///
/// AMAÇ: Access token yenileme isteği modeli.
/// NEDEN: POST /api/v1/auth/refresh body'sini modellemek.
/// BAĞIMLILIKLAR: IAuthService.RefreshAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Refresh token yenileme isteği.
///
/// AMAÇ: Süresi dolmuş access token'ı yenilemek için refresh token göndermek.
/// NEDEN: Token Family Pattern — her refresh işlemi eski token'ı geçersizleştirir.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// İstemcinin güvenli depolama alanından okuduğu ham refresh token.
    /// NEDEN: Sunucu bu değeri SHA-256 ile hashleyerek veritabanıyla karşılaştırır.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
