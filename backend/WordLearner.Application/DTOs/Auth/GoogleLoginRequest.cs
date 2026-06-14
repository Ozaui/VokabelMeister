/// <summary>
/// GoogleLoginRequest.cs
///
/// AMAÇ: Google Sign-In isteği modeli.
/// NEDEN: İstemci Google SDK'dan aldığı ID token'ı bu DTO ile gönderir.
///        Sunucu token'ı Google.Apis.Auth ile doğrular, kendi JWT'sini üretir.
/// BAĞIMLILIKLAR: IAuthService.GoogleLoginAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Google ile giriş isteği.
///
/// AMAÇ: POST /api/v1/auth/google body'sini modellemek.
/// NEDEN: Google token doğrulaması sunucuda yapılır — istemci token'ı doğrulayamaz.
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// Google SDK'nın döndürdüğü ID Token (JWT formatında).
    /// NEDEN: Sunucu Google Public Key'lerle imzayı doğrular → kullanıcı kimliği güvenli.
    /// </summary>
    public string IdToken { get; init; } = string.Empty;
}
