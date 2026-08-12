using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

// Apple'ın Google.Apis.Auth'a denk resmi bir .NET doğrulama SDK'sı yok — JWKS'i (Apple'ın imzalama
// anahtar seti) kendimiz çekip imzayı JwtSecurityTokenHandler ile elle doğruluyoruz (SECURITY.md §1.2).
public class AppleTokenValidator : IAppleTokenValidator
{
    private const string Issuer = "https://appleid.apple.com";
    private const string JwksUrl = "https://appleid.apple.com/auth/keys";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _cfg;

    public AppleTokenValidator(HttpClient httpClient, IConfiguration cfg)
    {
        _httpClient = httpClient;
        _cfg = cfg;
    }

    public async Task<AppleTokenPayload?> ValidateAsync(string identityToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var jwksJson = await _httpClient.GetStringAsync(JwksUrl, cancellationToken);
            var signingKeys = new JsonWebKeySet(jwksJson).GetSigningKeys();

            var principal = new JwtSecurityTokenHandler().ValidateToken(identityToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = _cfg["Apple:BundleId"],
                ValidateLifetime = true
            }, out _);

            var subject = principal.FindFirst("sub")?.Value;
            return subject is null ? null : new AppleTokenPayload(subject, principal.FindFirst("email")?.Value);
        }
        catch
        {
            return null;
        }
    }
}
