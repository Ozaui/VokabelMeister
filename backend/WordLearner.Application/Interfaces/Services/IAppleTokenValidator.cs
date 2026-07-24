namespace WordLearner.Application.Interfaces.Services;

// Email yalnızca İLK yetkilendirmede gelir (Apple'ın kısıtı) — sonraki girişlerde null
// olabilir, AuthService bu durumda DB'deki mevcut email'i korur.
public record AppleTokenPayload(string AppleId, string? Email);

public interface IAppleTokenValidator
{
    Task<AppleTokenPayload?> ValidateAsync(string identityToken, CancellationToken ct = default);
}
