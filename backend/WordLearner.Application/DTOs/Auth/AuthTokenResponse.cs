namespace WordLearner.Application.DTOs.Auth;

public record AuthUserDto(int Id, string CurrentLevel, string ThemePreference, string LanguagePreference);

public record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserDto User,
    bool AccountWasRecovered
);
