using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

// QR ile giriş akışının tek bir denemesini tutar (SECURITY.md §1.3); Confirmed durumu
// bir kez okunduktan sonra Consumed'a geçer, tek kullanımlıktır.
public class QrLoginSession : BaseEntity
{
    public string QrTokenHash { get; set; } = string.Empty;

    // Web/mobil ekranda yan yana gösterilen 4 haneli karşılaştırma kodu — TokenHash'ten
    // bağımsız bir savunma katmanı (relay/phishing saldırısına karşı).
    public string PairingCode { get; set; } = string.Empty;

    public QrLoginStatus Status { get; set; } = QrLoginStatus.Pending;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string? RequesterIp { get; set; }
    public string? RequesterDeviceInfo { get; set; }
    public DateTime? ScannedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
