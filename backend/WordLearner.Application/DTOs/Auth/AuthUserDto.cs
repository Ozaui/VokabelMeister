namespace WordLearner.Application.DTOs.Auth;

// API_ENDPOINTS.md §3 login yanıtındaki "user" alanıyla birebir — JWT claim'lerinden FAZLASINI
// taşımaz (Theme/LanguagePreference JWT'ye hiç girmez, yalnızca bu DTO'dan okunur).
public record AuthUserDto(int Id, string CurrentLevel, string ThemePreference, string LanguagePreference);
