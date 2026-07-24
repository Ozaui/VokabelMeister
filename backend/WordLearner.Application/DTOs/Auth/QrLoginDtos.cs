namespace WordLearner.Application.DTOs.Auth;

public record QrGenerateResponse(string QrToken, string PairingCode, int ExpiresIn);

public record QrScanResponse(string? RequesterDeviceInfo, string? RequesterIp, string PairingCode);

// Confirmed dışındaki durumlarda yalnızca Status doludur; Confirmed'de (tek seferlik) token alanları da gelir.
public record QrStatusResponse(
    string Status,
    string? AccessToken,
    string? RefreshToken,
    int? ExpiresIn,
    AuthUserDto? User
);
