namespace Zausel.Application.Interfaces.Services;

public record GoogleTokenPayload(string Subject, string Email, string? FirstName, string? LastName);

// Google.Apis.Auth'un GoogleJsonWebSignature.ValidateAsync'ini (statik, mock'lanamaz) bir arayüz
// arkasına alır — CODING_STANDARDS.md §6.4 "dış servisler (Google/Apple) her zaman mock" kuralı.
public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
