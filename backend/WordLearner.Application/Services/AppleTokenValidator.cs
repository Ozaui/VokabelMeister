using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

// Google'ın aksine Apple için resmi bir .NET doğrulama kütüphanesi yok — JWKS'ten anahtarları
// çekip identity token'ın imzasını/issuer/audience'ını elle doğrularız.
public class AppleTokenValidator : IAppleTokenValidator
{
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string AppleJwksUrl = "https://appleid.apple.com/auth/keys";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AppleTokenValidator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

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

            // Algorithm Confusion önlemi — imzalayan algoritmanın gerçekten RS256 olduğu elle teyit edilir.
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
            return null;
        }
    }
}
