using WordLearner.Domain.Entities;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

// BaseEntity'den türer — DATABASE_SCHEMA/Auth.md'deki QrLoginSessions tablosu BaseEntity'nin
// tüm alanlarını (kim yaptı dahil) birebir taşıyor, User/RefreshToken'ın aksine.
public class QrLoginSession : BaseEntity
{
    public string QrTokenHash { get; set; } = string.Empty;
    public string PairingCode { get; set; } = string.Empty;
    public QrLoginStatus Status { get; set; } = QrLoginStatus.Pending;
    public int? UserId { get; set; }
    public string? RequesterIp { get; set; }
    public string? RequesterDeviceInfo { get; set; }
    public DateTime? ScannedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
