/// <summary>
/// ITokenService.cs
///
/// AMAÇ: JWT access token ve kriptografik refresh token üretim sözleşmesi.
/// NEDEN: Token üretimi business logic değil — ayrı servis olarak mock ile test edilebilir.
///        JwtTokenService, IConfiguration'dan JWT ayarlarını okur; bu arayüz onu soyutlar.
/// BAĞIMLILIKLAR:
///   - User entity (access token claim'leri için)
///   - RefreshTokenResult DTO (token + süre birlikte taşınır)
/// </summary>

using System.Security.Claims;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Services;

/// <summary>
/// JWT ve refresh token üretiminin arayüzü.
///
/// AMAÇ: AuthService'in token üretim detaylarından bağımsız kalması.
/// NEDEN: Token imzalama anahtarı veya algoritma değişirse yalnızca implementasyon değişir.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// AMAÇ: Kullanıcı bilgileriyle JWT access token üretir.
    /// NEDEN: Token 15 dakika geçerli — kısa ömür güvenlik riskini azaltır.
    /// NASIL: HMAC-SHA256 ile imzalanır; payload'a UserId, Email, Role, FirstName eklenir.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// AMAÇ: Kriptografik güvenli refresh token üretir ve son kullanma tarihiyle birlikte döndürür.
    /// NEDEN: Son kullanma tarihi üretici (ITokenService) tarafından belirlenmeli.
    ///        AuthService bağımsız hesaplarsa config politikasıyla tutarsızlık çıkabilir.
    /// NASIL: RandomNumberGenerator ile 64 byte → Base64 string (88 karakter).
    ///        ExpiresAt = şimdi + Jwt:RefreshTokenExpirationDays (config'den).
    ///        Dönen Token ham değerdir; veritabanına SHA-256 hash'i kaydedilir.
    /// </summary>
    RefreshTokenResult GenerateRefreshToken();

    /// <summary>
    /// AMAÇ: Süresi dolmuş JWT'den ClaimsPrincipal çıkarır.
    /// NEDEN: Token yenileme akışında kullanıcıyı tanımlamak için kullanılabilir.
    ///        ValidateLifetime=false ile süresi dolmuş token'ı okur.
    ///
    /// GÜVENLİK — JWT Algorithm Confusion Attack önlemi:
    ///   Saldırgan token header'ını manipüle edebilir:
    ///     - alg: "none"  → imza doğrulaması atlatılır
    ///     - RS256 → HS256 → sunucunun public key'i HMAC sırrı gibi kullanılır
    ///   Önlem: ValidateToken sonrası SecurityToken'ın algoritması kod seviyesinde
    ///          HS256 (HmacSha256) olduğu if kontrolüyle zorunlu doğrulanır.
    ///          Eşleşmezse null döner — token geçersiz sayılır.
    ///
    /// NASIL: JwtSecurityTokenHandler.ValidateToken → imza doğrulanır, süre atlanır.
    ///        Ardından: jwtToken.Header.Alg == SecurityAlgorithms.HmacSha256 kontrolü.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
