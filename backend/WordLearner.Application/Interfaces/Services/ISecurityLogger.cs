
// ─────────────────────────────────────────────────────────────────────────────
// ISecurityLogger.cs
//
// AMAÇ: Güvenlik olayı kaydı yazma sözleşmesi (LoginFailed/OtpFailed/RateLimitHit
//       vb. — bkz. REFERENCE/SECURITY.md §6).
// NEDEN: Handler'lar SecurityLog entity'sini/EmailHash üretimini doğrudan bilmemeli —
//        bu arayüz e-posta hash'lemeyi (PII kuralı: ham e-posta asla loglanmaz) ve
//        repository çağrısını tek yerde saklar.
// BAĞIMLILIKLAR: LogEventType enum.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Interfaces.Services;

public interface ISecurityLogger
{
    // AMAÇ: Bir güvenlik olayı kaydı yazar.
    // NASIL: email verilirse SHA-256(email)→Base64 hash'i EmailHash'e yazılır
    //        (SECURITY.md §6 "PII" kuralı — ham e-posta asla saklanmaz); email
    //        biliniyorsa bile UserId ayrıca verilmelidir (ör. kayıtlı olmayan bir
    //        e-postayla login denemesinde UserId=null, EmailHash yine de dolar).
    Task LogAsync(
        LogEventType eventType,
        int? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? detail = null,
        CancellationToken ct = default
    );
}
