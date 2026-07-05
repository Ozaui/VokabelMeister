// ─────────────────────────────────────────────────────────────────────────────
// AppleTokenValidator.cs
//
// AMAÇ: IAppleTokenValidator'ın JWKS tabanlı implementasyonu.
// NEDEN: Google'ın aksine Apple için resmi bir .NET doğrulama kütüphanesi yok —
//        Apple'ın herkese açık anahtarlarını (JWKS) https://appleid.apple.com/auth/keys'ten
//        çekip identity token'ın imzasını, issuer'ını ve audience'ını (Apple:BundleId,
//        REFERENCE/ENV.md §4) elle doğrularız.
// BAĞIMLILIKLAR: System.IdentityModel.Tokens.Jwt, Microsoft.IdentityModel.Tokens,
//                System.Net.Http.Json, Microsoft.Extensions.Configuration.
// ─────────────────────────────────────────────────────────────────────────────

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class AppleTokenValidator : IAppleTokenValidator
{
    // NEDEN sabit: Apple'ın kimlik sağlayıcı adresi hiç değişmez, appsettings'e taşımaya gerek yok.
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string AppleJwksUrl = "https://appleid.apple.com/auth/keys";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AppleTokenValidator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // AMAÇ: Apple identity token'ını doğrular, geçerliyse kullanıcı bilgilerini döner.
    // NEDEN: Apple e-postayı yalnızca İLK yetkilendirmede identity token'ın içine koyar;
    //        sonraki girişlerde "email" claim'i olmayabilir — bu normaldir, AuthService
    //        bu durumda DB'deki mevcut Users.Email'i korur, üzerine yazmaz.
    // NASIL: 1) Apple'ın JWKS'sini çek  2) Token'ı bu anahtarlarla + issuer/audience'a
    //        göre doğrula  3) "sub" (AppleId) ve varsa "email" claim'lerini çıkar.
    public async Task<AppleTokenPayload?> ValidateAsync(
        string identityToken,
        CancellationToken ct = default
    )
    {
        try
        {
            var jwks = await _httpClient.GetFromJsonAsync<JsonWebKeySet>(AppleJwksUrl, ct);
            if (jwks is null)
                return null;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudience = _configuration["Apple:BundleId"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(
                identityToken,
                validationParameters,
                out var validatedToken
            );

            // NEDEN: JwtTokenService.GetPrincipalFromExpiredToken'daki Algorithm Confusion
            //        önlemiyle aynı gerekçe — imzalanan algoritmanın gerçekten Apple'ın
            //        kullandığı RS256 olduğu elle teyit edilir.
            if (
                validatedToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.RsaSha256,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return null;

            var appleId = principal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(appleId))
                return null;

            var email = principal.FindFirst("email")?.Value;
            return new AppleTokenPayload(appleId, email);
        }
        catch
        {
            // NEDEN: JWKS çekilemezse (ağ hatası) veya token doğrulama başarısız olursa
            //        (imza/issuer/audience/süre) AuthService'e null dönüp karar verdiririz.
            return null;
        }
    }
}
