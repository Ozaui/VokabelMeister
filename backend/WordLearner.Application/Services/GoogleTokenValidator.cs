using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration) => _configuration = configuration;

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
            return null;
        }
    }
}
