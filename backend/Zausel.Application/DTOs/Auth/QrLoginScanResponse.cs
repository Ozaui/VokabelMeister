namespace Zausel.Application.DTOs.Auth;

// Mobilde gösterilir — kullanıcı requesterDeviceInfo/requesterIp'i kendi web/admin oturumuyla
// gözle karşılaştırıp onaylar (SECURITY.md §1.3, relay/phishing önlemi).
public record QrLoginScanResponse(string? RequesterDeviceInfo, string? RequesterIp, string PairingCode);
