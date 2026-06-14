/// <summary>
/// AppleLoginRequest.cs
///
/// AMAÇ: Apple Sign In isteği modeli.
/// NEDEN: Apple yalnızca ilk girişte kullanıcı adını döndürür — sonraki girişlerde null gelir.
///        Bu yüzden FirstName ve LastName opsiyoneldir.
/// BAĞIMLILIKLAR: IAuthService.AppleLoginAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Apple Sign In isteği.
///
/// AMAÇ: POST /api/v1/auth/apple body'sini modellemek.
/// NEDEN: Apple identity token ES256 ile imzalanmış JWT'dir.
///        Sunucu Apple JWKS endpoint'inden public key alarak doğrular.
/// </summary>
public class AppleLoginRequest
{
    /// <summary>
    /// Apple SDK'nın döndürdüğü Identity Token (JWT, ES256 imzalı).
    /// NEDEN: Subject (Apple User ID) ve e-posta buradan çıkarılır.
    /// </summary>
    public string IdentityToken { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının adı — yalnızca ilk girişte Apple tarafından gönderilir.
    /// NEDEN: Apple sonraki girişlerde ismi tekrar vermez; ilk girişte kaydedilmeli.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Kullanıcının soyadı — yalnızca ilk girişte Apple tarafından gönderilir.
    /// </summary>
    public string? LastName { get; init; }
}
