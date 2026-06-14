/// <summary>
/// ResetPasswordRequest.cs
///
/// AMAÇ: Şifre sıfırlama işlemini tamamlama isteği modeli.
/// NEDEN: E-posta ile gönderilen 6 haneli OTP kodu + yeni şifre birlikte gönderilir.
///        Link tabanlı sıfırlama yerine kod tabanlı akış tercih edildi:
///        mobil uygulamalarda link açma güvenilmezdir (e-posta istemcisi, deep link vb.),
///        kod girilmesi UX açısından daha pürüzsüz çalışır.
/// BAĞIMLILIKLAR: IAuthService.ResetPasswordAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Şifre sıfırlama tamamlama isteği.
///
/// AMAÇ: POST /api/v1/auth/reset-password body'sini modellemek.
/// NEDEN: Sunucu önce e-postadan kullanıcıyı bulur, ardından kodu doğrular.
///        E-posta ayrı gönderildiği için kod tek başına anonymous lookup gerektirmez.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Şifresi sıfırlanacak hesabın e-posta adresi.
    /// NEDEN: Kullanıcı kaydını bulmak için kullanılır; kod tek başına yeterli değildir.
    ///        E-posta + kod çifti brute force riskini azaltır.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// E-posta ile gönderilen 6 haneli tek kullanımlık kod (OTP).
    /// KISIT: 5 dakika geçerli, tek kullanım — doğrulamadan sonra geçersizleştirilir.
    /// GÜVENLİK: SHA-256 hash'i veritabanında saklanır; brute force için rate limiting zorunlu.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Belirlenmek istenen yeni şifre.
    /// KISIT: Min 12 karakter, büyük/küçük harf, rakam, özel karakter zorunlu.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
}
