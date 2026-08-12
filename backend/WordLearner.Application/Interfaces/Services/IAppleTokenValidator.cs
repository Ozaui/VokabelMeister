namespace WordLearner.Application.Interfaces.Services;

public record AppleTokenPayload(string Subject, string? Email);

public interface IAppleTokenValidator
{
    Task<AppleTokenPayload?> ValidateAsync(string identityToken, CancellationToken cancellationToken = default);
}
