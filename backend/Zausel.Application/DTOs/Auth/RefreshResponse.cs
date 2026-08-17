namespace Zausel.Application.DTOs.Auth;

// Refresh, User nesnesini yeniden döndürmez — istemci zaten oturum açık, yalnızca yeni token çifti gerekir.
public record RefreshResponse(string AccessToken, string RefreshToken, int ExpiresIn);
