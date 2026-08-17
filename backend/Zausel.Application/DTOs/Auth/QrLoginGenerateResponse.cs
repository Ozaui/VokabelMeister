namespace Zausel.Application.DTOs.Auth;

// qrToken ham değeri yalnızca BURADA döner (DB'de yalnızca hash'i var) — QR/deep-link içine gömülür.
public record QrLoginGenerateResponse(string QrToken, string PairingCode, int ExpiresIn);
