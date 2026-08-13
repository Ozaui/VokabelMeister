namespace WordLearner.Application.DTOs.Auth;

// FirstName/LastName opsiyonel — Apple bunu yalnızca cihazda İLK izin anında gönderir (LoginWithAppleCommand'ın notu).
public record LoginWithAppleRequest(string IdentityToken, string? FirstName, string? LastName);
