/// <summary>
/// IAppleTokenValidator.cs
///
/// AMAÇ: Apple identity token doğrulama sözleşmesi.
/// NEDEN: Apple token doğrulaması HTTP çağrısı gerektirir (JWKS endpoint).
///        Bu, Application katmanında değil Infrastructure katmanında yapılmalı.
///        Bu arayüz Application'ın Infrastructure'a bağımlılığını tersine çevirir.
///
/// IMPLEMENTASYON: Infrastructure/Services/Auth/AppleTokenValidator.cs — TASK-005
///   - Apple JWKS: https://appleid.apple.com/auth/keys
///   - Token algoritması: ES256 (asimetrik — RS256 değil)
///   - Audience: appsettings.json → "Apple:BundleId"
///
/// BAĞIMLILIKLAR: AppleTokenPayload DTO
/// </summary>

using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Interfaces.Services;

/// <summary>
/// Apple Sign In identity token doğrulama arayüzü.
///
/// AMAÇ: AuthService'in Apple'a HTTP isteği atma detayından bağımsız kalması.
/// NEDEN: Apple JWKS anahtarları zaman zaman değişir — cache stratejisi
///        Infrastructure'da yönetilmeli, AuthService bilmemeli.
/// </summary>
public interface IAppleTokenValidator
{
    /// <summary>
    /// AMAÇ: Apple'ın identity token'ını doğrular ve kullanıcı bilgilerini çıkarır.
    /// NEDEN: Apple ile giriş güvenli olabilmesi için token Apple public key'leriyle doğrulanmalı.
    /// NASIL:
    ///   1. Token header'ından kid alınır
    ///   2. Apple JWKS endpoint'inden eşleşen key getirilir (önbellekten)
    ///   3. JWT imzası doğrulanır (ES256)
    ///   4. iss = "https://appleid.apple.com" ve aud = Apple:BundleId kontrolü
    ///   5. Subject ve email claim'leri döndürülür
    /// </summary>
    /// <exception cref="Exceptions.InvalidCredentialsException">Token geçersizse fırlatılır.</exception>
    Task<AppleTokenPayload> ValidateAsync(string identityToken, CancellationToken ct = default);
}
