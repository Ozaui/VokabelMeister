/// <summary>
/// VerifyEmailRequest.cs
///
/// AMAÇ: E-posta doğrulama isteği modeli.
/// NEDEN: POST /api/v1/auth/verify-email body'sini modellemek.
///        Kullanıcı kayıt sonrası aldığı 6 haneli kodu bu endpoint ile gönderir.
/// BAĞIMLILIKLAR: IAuthService.VerifyEmailAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// E-posta doğrulama isteği.
///
/// AMAÇ: Kayıt sonrası e-posta adresini 6 haneli OTP koduyla doğrulamak.
/// NEDEN: E-posta + kod çifti brute force riskini düşürür; kod tek başına anonymous değildir.
/// </summary>
public class VerifyEmailRequest
{
    /// <summary>
    /// Doğrulanacak e-posta adresi.
    /// NEDEN: Kullanıcı kaydını bulmak için kullanılır.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Kayıt e-postasıyla gönderilen 6 haneli OTP kodu.
    /// KISIT: 24 saat geçerli, tek kullanım — doğrulamadan sonra geçersizleştirilir.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}
