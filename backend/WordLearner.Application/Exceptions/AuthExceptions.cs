/// <summary>
/// AuthExceptions.cs
///
/// AMAÇ: Kimlik doğrulama akışında fırlatılan özel exception sınıfları.
/// NEDEN: Global exception middleware (TASK-006) bu tiplere göre HTTP yanıt kodlarını belirler.
///        Tipleştirilmiş exception'lar "ne yanlış gitti" bilgisini açıkça taşır.
/// BAĞIMLILIKLAR: -
/// </summary>

namespace WordLearner.Application.Exceptions;

/// <summary>
/// Tüm kimlik doğrulama exception'larının temel sınıfı.
///
/// AMAÇ: Global middleware'in auth hatalarını kolayca yakalamak için tek tip.
/// NEDEN: AuthService'ten fırlatılan farklı hatalar ayrı HTTP kodlarına dönüşür.
/// </summary>
public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
}

/// <summary>
/// E-posta adresi zaten kayıtlıyken kayıt denemesi.
/// NEDEN: HTTP 409 Conflict döndürülmeli — kayıt formu "Bu e-posta kullanılıyor" mesajı gösterir.
/// </summary>
public class EmailAlreadyExistsException : AuthException
{
    public EmailAlreadyExistsException(string message) : base(message) { }
}

/// <summary>
/// Yanlış e-posta veya şifre ile giriş denemesi.
/// NEDEN: HTTP 401 Unauthorized — mesaj kasıtlı belirsiz ("E-posta veya şifre hatalı").
///        Hangi alanın yanlış olduğu belirtilmez — kullanıcı adı sayım saldırısı önlenir.
/// </summary>
public class InvalidCredentialsException : AuthException
{
    public InvalidCredentialsException(string message) : base(message) { }
}

/// <summary>
/// Devre dışı bırakılmış hesaba giriş denemesi.
/// NEDEN: HTTP 403 Forbidden — admin hesabı kilitlediğinde kullanıcıya açıklama gösterilir.
/// </summary>
public class AccountDisabledException : AuthException
{
    public AccountDisabledException(string message) : base(message) { }
}

/// <summary>
/// Geçersiz veya süresi dolmuş refresh token ile yenileme denemesi.
/// NEDEN: HTTP 401 — mobil uygulama kullanıcıyı giriş sayfasına yönlendirir.
/// </summary>
public class InvalidRefreshTokenException : AuthException
{
    public InvalidRefreshTokenException(string message) : base(message) { }
}

/// <summary>
/// Geçersiz veya süresi dolmuş şifre sıfırlama kodu.
/// NEDEN: HTTP 400 Bad Request — "Kod geçersiz veya süresi dolmuş" mesajı gösterilir.
/// </summary>
public class InvalidResetTokenException : AuthException
{
    public InvalidResetTokenException(string message) : base(message) { }
}

/// <summary>
/// E-posta adresi doğrulanmamış kullanıcı giriş yapmaya çalıştığında.
/// NEDEN: HTTP 403 Forbidden — "E-postanızı doğrulamanız gerekiyor" mesajı gösterilir.
///        Yanıt gövdesinde yeni doğrulama kodunun gönderildiği bildirilir.
/// </summary>
public class EmailNotVerifiedException : AuthException
{
    public EmailNotVerifiedException(string message) : base(message) { }
}

/// <summary>
/// Geçersiz veya süresi dolmuş e-posta doğrulama kodu.
/// NEDEN: HTTP 400 Bad Request — kullanıcı yanlış kod girdiğinde gösterilir.
/// </summary>
public class InvalidVerificationCodeException : AuthException
{
    public InvalidVerificationCodeException(string message) : base(message) { }
}
