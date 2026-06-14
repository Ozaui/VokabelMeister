/// <summary>
/// LogoutRequest.cs
///
/// AMAÇ: Çıkış isteği modeli — refresh token'ı sunucuda iptal etmek için.
/// NEDEN: POST /api/v1/auth/logout body'sini modellemek.
///        Sunucu token'ı DB'de iptal etmeden istemci silerse token başka yerden kullanılabilir.
/// BAĞIMLILIKLAR: IAuthService.LogoutAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Çıkış isteği.
///
/// AMAÇ: İstemcinin refresh token'ını sunucuda geçersizleştirmek.
/// NEDEN: Yalnızca istemci taraflı silme güvensizdir — token çalınmışsa hâlâ geçerli kalır.
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// İptal edilecek ham refresh token.
    /// NEDEN: Sunucu bu token'ın hash'ini bularak RevokedAt=şimdi yapar.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
