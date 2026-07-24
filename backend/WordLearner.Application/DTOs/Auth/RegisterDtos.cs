namespace WordLearner.Application.DTOs.Auth;

// Token dönmez — kayıt sonrası e-posta doğrulaması gerekir. CurrentLevel/ThemePreference
// yalnızca DB varsayılanını (A1/System) döner, gerçek seçim ilk-login-sonrası onboarding'de yapılır.
public record RegisterResponse(
    int Id,
    string Email,
    string FirstName,
    string CurrentLevel,
    string ThemePreference
);
