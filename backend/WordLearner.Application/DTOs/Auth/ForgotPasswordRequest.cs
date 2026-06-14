/// <summary>
/// ForgotPasswordRequest.cs
///
/// AMAÇ: Şifre sıfırlama isteği başlatma modeli.
/// NEDEN: POST /api/v1/auth/forgot-password body'sini modellemek.
/// BAĞIMLILIKLAR: IAuthService.ForgotPasswordAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Şifre sıfırlama isteği.
///
/// AMAÇ: Kullanıcının e-posta adresiyle şifre sıfırlama akışını başlatmak.
/// NEDEN: Sunucu e-posta var mı yok mu bilgisini vermez — her durumda "gönderildi" yanıtı döner.
///        Bu sayede kullanıcı hesabı sayım saldırısı (user enumeration) önlenir.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Şifresi sıfırlanacak hesabın e-posta adresi.
    /// GÜVENLİK: E-posta sistemde yoksa bile "e-posta gönderildi" yanıtı döner.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
