/// <summary>
/// LoginRequest.cs
///
/// AMAÇ: E-posta ve şifre ile giriş isteği modeli.
/// NEDEN: POST /api/v1/auth/login body'sini modellemek.
/// BAĞIMLILIKLAR: IAuthService.LoginAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Giriş isteği — e-posta + şifre çifti.
///
/// AMAÇ: Login akışında kimlik bilgilerini controller'dan servise taşımak.
/// NEDEN: Controller IP adresi ve device info'yu HTTP context'ten ekler; bu DTO saf girdiyi taşır.
/// </summary>
public class LoginRequest
{
    /// <summary>Kullanıcı e-posta adresi.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Ham şifre — doğrulama sonrası hiçbir yerde saklanmaz.
    /// NEDEN: BCrypt.Verify ile hashlenen değerle karşılaştırılır; plain text log'a yazılmaz.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
