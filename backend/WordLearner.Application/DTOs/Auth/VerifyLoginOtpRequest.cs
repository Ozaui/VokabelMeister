/// <summary>
/// VerifyLoginOtpRequest.cs
///
/// AMAÇ: Local hesap girişinin 2. adımı — OTP kodu doğrulama isteği.
/// NEDEN: Local girişte 2FA zorunludur:
///        Adım 1: POST /auth/login → şifre doğrula → OTP gönder
///        Adım 2: POST /auth/login/verify-otp → OTP doğrula → token döndür
///        Bu DTO Adım 2'nin body'sidir.
/// BAĞIMLILIKLAR: IAuthService.LoginVerifyOtpAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Giriş OTP doğrulama isteği.
///
/// AMAÇ: POST /api/v1/auth/login/verify-otp body'sini modellemek.
/// NEDEN: OTP tek başına anonim değil — e-posta ile kullanıcı kaydı bulunur.
/// </summary>
public class VerifyLoginOtpRequest
{
    /// <summary>Giriş yapan kullanıcının e-posta adresi.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// E-posta ile gönderilen 6 haneli giriş OTP kodu.
    /// KISIT: 5 dakika geçerli, tek kullanım.
    /// RATE LIMIT: TASK-006'da 5 yanlış deneme → kod geçersizleştirilir.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}
