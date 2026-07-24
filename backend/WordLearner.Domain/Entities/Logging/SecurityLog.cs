using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Domain.Entities.Logging;

// Güvenlik olayı kaydı (başarısız login, rate-limit, yetkisiz erişim...) — BaseEntity'den
// TÜREMEZ (ActivityLog ile aynı gerekçe). ActivityLog başarılı işlemleri, bu tablo
// hukuka/güvenliğe aykırı olabilecek (çoğunlukla başarısız) olayları tutar.
public class SecurityLog
{
    public long Id { get; set; }
    public LogEventType EventType { get; set; }

    // Biliniyorsa dolar (ör. LoginFailed'de e-posta bulunduysa); bulunamazsa NULL.
    public int? UserId { get; set; }

    // Kimlik doğrulanmadan ilişkilendirme için ham e-postanın SHA-256 hash'i.
    public string? EmailHash { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Serbest metin DEĞİL, sabit bir Code — admin GET /admin/logs/* ile OKURKEN kendi
    // Accept-Language'ıyla çözülür (CLAUDE.md §1 "İkinci istisna").
    public string? Detail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}
