// ─────────────────────────────────────────────────────────────────────────────
// AdminLogDtos.cs
//
// AMAÇ: `GET /admin/logs/activity|application|security` yanıtlarının DTO'ları.
// NEDEN: Entity'lerin BİREBİR aynısı DEĞİL — `SecurityLogDto.Detail` ÇÖZÜLMÜŞ
//        (LogMessages.Resolve ile tr/de'ye çevrilmiş) metni taşır, ham Code'u DEĞİL;
//        `SecurityLogDto.EventType` enum DEĞİL string (QrStatusResponse'un
//        `session.Status.ToString()` deseniyle AYNI — enum'lar JSON'a int olarak
//        serileşir, STJ'ye JsonStringEnumConverter kayıtlı değil, admin panelin
//        "LoginFailed" gibi okunabilir bir string BEKLEMESİ için elle ToString()).
//        `ActivityLogDto` OldValue/NewValue'yu HAM JSON string olarak taşır — bkz.
//        LogMessages.cs "NEDEN yalnızca Detail" notu.
// BAĞIMLILIKLAR: Yok (saf DTO'lar).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Admin;

// AMAÇ: `GET /admin/logs/activity` satırı.
public record ActivityLogDto(
    long Id,
    int? UserId,
    string? ActorRole,
    string Action,
    string? EntityType,
    int? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);

// AMAÇ: `GET /admin/logs/application` satırı.
public record ApplicationLogDto(
    long Id,
    string Level,
    string Message,
    string? Exception,
    string? SourceContext,
    string? RequestPath,
    int? UserId,
    DateTime TimeStamp
);

// AMAÇ: `GET /admin/logs/security` satırı — `Detail`, admin isteğinin dilinde ÇÖZÜLMÜŞ
//       metni taşır (bkz. LogMessages.cs).
public record SecurityLogDto(
    long Id,
    string EventType,
    int? UserId,
    string? IpAddress,
    string? UserAgent,
    string? Detail,
    DateTime CreatedAt
);
