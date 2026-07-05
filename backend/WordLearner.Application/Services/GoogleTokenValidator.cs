// ─────────────────────────────────────────────────────────────────────────────
// GoogleTokenValidator.cs
//
// AMAÇ: IGoogleTokenValidator'ın Google.Apis.Auth kütüphanesi tabanlı implementasyonu.
// NEDEN: Backend, Google'ın client secret'ına ihtiyaç duymadan yalnızca ID token'ın
//        imzasını ve audience'ını (Google:ClientId) doğrular — REFERENCE/ENV.md §3.
// BAĞIMLILIKLAR: Google.Apis.Auth, Microsoft.Extensions.Configuration.
// ─────────────────────────────────────────────────────────────────────────────

using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration) => _configuration = configuration;

    // AMAÇ: Google ID token'ını doğrular, geçerliyse kullanıcı bilgilerini döner.
    // NEDEN: GoogleJsonWebSignature.ValidateAsync Google'ın JWKS'sinden anahtarları
    //        kendi indirir/önbellekler; biz yalnızca audience'ın bizim Client Id'mize
    //        eşit olduğunu doğrulatırız (aksi hâlde başka bir uygulama için üretilmiş
    //        bir token da kabul edilirdi).
    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"]! },
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenPayload(payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
        }
        catch (InvalidJwtException)
        {
            // NEDEN: İmza geçersiz, süre dolmuş veya audience uyuşmuyor — hepsi bu tek
            //        exception tipinde toplanır; AuthService'e null dönüp karar verdiririz.
            return null;
        }
    }
}
