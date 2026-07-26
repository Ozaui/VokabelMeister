namespace WordLearner.Application.DTOs.Auth;

// Token dönmez — kayıt sonrası e-posta doğrulaması gerekir. CurrentLevel/ThemePreference/
// LanguagePreference yalnızca DB varsayılanını (A1/System/tr) döner, gerçek seçim ilk-login-sonrası
// onboarding'de yapılır.
public record RegisterResponse(
    int Id,
    string Email,
    string FirstName,
    string CurrentLevel,
    string ThemePreference,
    string LanguagePreference
);
