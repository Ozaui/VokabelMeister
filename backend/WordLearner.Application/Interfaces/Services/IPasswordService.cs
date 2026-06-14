/// <summary>
/// IPasswordService.cs
///
/// AMAÇ: Şifre hashleme ve doğrulama sözleşmesi.
/// NEDEN: BCrypt implementasyonu Application katmanında — test edilebilir ve değiştirilebilir.
///        Şifre sıfırlama ve refresh token'lar için SHA-256 yardımcısı da burada.
/// BAĞIMLILIKLAR: -
/// </summary>

namespace WordLearner.Application.Interfaces.Services;

/// <summary>
/// Şifre ve token güvenlik operasyonlarının arayüzü.
///
/// AMAÇ: BCrypt şifre hashleme ve SHA-256 token hashleme işlemlerini soyutlamak.
/// NEDEN: AuthService doğrudan BCrypt'e bağımlı olmaz — mock ile test edilebilir.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// AMAÇ: Kullanıcı şifresini BCrypt ile hashler.
    /// NEDEN: Veritabanında düz metin şifre asla saklanmaz.
    /// NASIL: BCrypt work factor 12 — GPU brute force saldırılarına karşı yeterince yavaş.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// AMAÇ: Kullanıcının girdiği şifreyi hashlenen ile karşılaştırır.
    /// NEDEN: BCrypt.Verify sabit zamanlı karşılaştırma yapar — timing attack önlenir.
    /// NASIL: BCrypt.Verify(plain, hash) → true/false
    /// </summary>
    bool Verify(string password, string hash);

    /// <summary>
    /// AMAÇ: Refresh token veya şifre sıfırlama token'ını SHA-256 ile hashler.
    /// NEDEN: Token veritabanında ham değil, SHA-256 hash olarak saklanır.
    ///        Veritabanı sızıntısında token'lar kullanılamaz hale gelir.
    /// NASIL: SHA-256(rawToken) → Base64 string (44 karakter)
    /// </summary>
    string HashToken(string rawToken);
}
