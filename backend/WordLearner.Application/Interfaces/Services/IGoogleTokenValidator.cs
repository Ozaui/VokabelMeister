namespace WordLearner.Application.Interfaces.Services;

public record GoogleTokenPayload(
    string GoogleId,
    string Email,
    string? FirstName,
    string? LastName
);

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken ct = default);
}
