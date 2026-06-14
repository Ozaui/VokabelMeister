/// <summary>
/// RegisterRequest.cs
///
/// AMAÇ: Yeni kullanıcı kaydı isteği modeli.
/// NEDEN: FluentValidation kuralları TASK-006'da bu DTO üzerine uygulanacak.
/// BAĞIMLILIKLAR: IAuthService.RegisterAsync
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Kayıt isteği — e-posta ve şifre ile yeni hesap oluşturma.
///
/// AMAÇ: POST /api/v1/auth/register body'sini modellemek.
/// NEDEN: Controller doğrudan entity almaz; DTO katmanı giriş verisini izole eder.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Kullanıcı e-posta adresi.
    /// KISIT: Benzersiz, max 254 karakter, geçerli e-posta formatı.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Ham şifre (düz metin) — servis katmanında BCrypt ile hashlenir, saklanmaz.
    /// KISIT: Min 12 karakter, büyük/küçük harf, rakam ve özel karakter zorunlu.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Ad — max 50 karakter.</summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Soyad — max 50 karakter.</summary>
    public string LastName { get; init; } = string.Empty;
}
