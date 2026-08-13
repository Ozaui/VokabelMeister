namespace WordLearner.Application.DTOs.Auth;

// Web/Admin'in ~2sn'de bir sorguladığı polling yanıtı. Status Confirmed dışında yalnızca "status"
// doludur; Confirmed İLK okunduğunda (tek seferlik) token alanları da doldurulur — SECURITY.md §1.3 ADIM 4.
public record QrLoginStatusResponse(
    string Status,
    string? AccessToken = null,
    string? RefreshToken = null,
    int? ExpiresIn = null,
    AuthUserDto? User = null,
    bool? AccountWasRecovered = null);
