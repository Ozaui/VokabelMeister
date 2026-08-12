using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _cfg;

    public GoogleTokenValidator(IConfiguration cfg) => _cfg = cfg;

    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_cfg["Google:ClientId"]!]
            });
            return new GoogleTokenPayload(payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
