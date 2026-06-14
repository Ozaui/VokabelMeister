/// <summary>
/// AuthResponse.cs
///
/// AMAÇ: Başarılı kimlik doğrulama işlemlerinin yanıt nesnesi.
/// NEDEN: Register, Login, GoogleLogin, AppleLogin ve Refresh endpoint'lerinin tümü
///        aynı yapıyı döndürür — istemci tek bir response modeli işler.
/// BAĞIMLILIKLAR: UserDto
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Kimlik doğrulama başarı yanıtı.
///
/// AMAÇ: Access token, refresh token ve kullanıcı profilini birlikte döndürmek.
/// NEDEN: Mobil uygulama giriş sonrası hem tokenları hem profili alır — iki ayrı istek gerekmez.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token — 15 dakika geçerli.
    /// NEDEN: Her API isteğinin Authorization: Bearer {token} header'ında gönderilir.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Ham refresh token — istemci güvenli depoda saklar (Keychain / Keystore).
    /// NEDEN: Access token süresi dolunca bu token ile yenilenebilir (7 gün geçerli).
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Access token'ın saniye cinsinden geçerlilik süresi (900 = 15 dakika).
    /// NEDEN: İstemci ne zaman refresh yapacağını hesaplamak için kullanır.
    /// </summary>
    public int ExpiresIn { get; init; } = 900;

    /// <summary>Giriş yapan kullanıcının profil bilgileri.</summary>
    public UserDto User { get; init; } = null!;

    /// <summary>
    /// Hesap grace period içindeyken giriş yapılıp kurtarıldıysa TRUE.
    /// NEDEN: Mobil uygulama "Hesabınız kurtarıldı" bildirimini gösterir.
    ///        Normal girişlerde her zaman FALSE.
    /// </summary>
    public bool AccountWasRecovered { get; init; } = false;
}
