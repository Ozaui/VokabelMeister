// ─────────────────────────────────────────────────────────────────────────────
// SecurityLog.cs
//
// AMAÇ: Güvenlik olayı kaydı (başarısız login, rate-limit, yetkisiz erişim vb.).
// NEDEN: BaseEntity'den TÜRETİLMEZ (ActivityLog.cs'teki gerekçeyle aynı — insert-only,
//        değişmez). `ISecurityLogger` bu tabloya yazar; ActivityLog'dan farkı, bunun
//        "hukuka/güvenliğe aykırı olabilecek" olayları (çoğunlukla kullanıcının kendi
//        BAŞARISIZ denemeleri) tutması — ActivityLog ise başarılı işlemlerin audit'idir.
// BAĞIMLILIKLAR: User (N:1, opsiyonel), LogEventType enum.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Domain.Entities.Logging;

public class SecurityLog
{
    public long Id { get; set; }

    // AMAÇ: Olayın türü (LoginFailed|OtpFailed|RateLimitHit|... — bkz. LogEventType).
    public LogEventType EventType { get; set; }

    // AMAÇ: Olayla ilişkili kullanıcı (biliniyorsa). Ör. LoginFailed'de kullanıcı e-postayla
    //       bulunduysa dolar; e-posta hiç kayıtlı değilse NULL kalır.
    public int? UserId { get; set; }

    // AMAÇ: Kullanıcı henüz kimliği doğrulanmadan (LoginFailed vb.) ilişkilendirme yapabilmek
    //       için ham e-postanın SHA-256 hash'i. NEDEN 44 karakter: IPasswordService.HashToken
    //       SHA-256(32 byte)→Base64 üretir, sabit 44 karakter (bkz. RefreshTokens.TokenHash/
    //       QrLoginSessions.QrTokenHash/Users.OriginalEmailHash ile aynı desen — Loglama.md'deki
    //       eski VARCHAR(88) değeri bu projenin genelinde 2026-07-11'de düzeltilen bir hataydı,
    //       SecurityLog o düzeltme sırasında henüz kodlanmadığı için gözden kaçmıştı).
    public string? EmailHash { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // AMAÇ: Olaya özel serbest metin ayrıntı (ör. "5 dakikada 6. başarısız deneme").
    public string? Detail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // AMAÇ: Navigation property — ActivityLog.User ile aynı gerekçeyle tek yönlü.
    public User? User { get; set; }
}
