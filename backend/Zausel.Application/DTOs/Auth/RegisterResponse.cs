namespace Zausel.Application.DTOs.Auth;

// API_ENDPOINTS.md §3: CurrentLevel/ThemePreference/LanguagePreference kayıt girdisi DEĞİL, DB
// varsayılanı — yanıtta yalnızca geri bildirim amaçlı döner.
public record RegisterResponse(int Id, string Email, string FirstName, string CurrentLevel, string ThemePreference, string LanguagePreference);
