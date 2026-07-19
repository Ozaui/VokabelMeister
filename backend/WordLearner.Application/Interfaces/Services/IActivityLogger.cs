// ─────────────────────────────────────────────────────────────────────────────
// IActivityLogger.cs
//
// AMAÇ: "Kim ne yaptı" audit kaydı yazma sözleşmesi — ileride yazılacak her
//       Command Handler'ın (CREATE_WORD/DELETE_USER_CARD/CHANGE_ROLE vb. — bkz.
//       CLAUDE.md "Veri katmanı") çağıracağı ortak servis.
// NEDEN: Handler'lar ActivityLog entity'sini/repository'sini doğrudan bilmemeli —
//        bu arayüz JSON serileştirmeyi (OldValue/NewValue) ve repository çağrısını
//        tek yerde saklar; her handler yalnızca "ne oldu"yu bildirir.
// BAĞIMLILIKLAR: Yok — saf sözleşme (parametreler primitive/object).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

public interface IActivityLogger
{
    // AMAÇ: Bir audit kaydı yazar.
    // NASIL: oldValue/newValue verilirse JSON'a serileştirilip OldValue/NewValue
    //        kolonlarına yazılır (ör. bir güncellemede eski/yeni entity durumu);
    //        yalnızca oluşturma/silme gibi tek durumlu eylemlerde bunlardan biri
    //        (genelde newValue) yeterlidir, ikisi de opsiyoneldir.
    // NEDEN userId/actorRole/ipAddress/userAgent ayrı ayrı parametre (bir "context"
    //       nesnesi değil): çağıran her Handler zaten bu bilgilere kendi elindeki
    //       HttpContext/Command'dan sahip — paylaşılan bir context tipi bu noktada
    //       gerçek bir tüketicisi olmayan spekülatif bir soyutlama olurdu (YAGNI).
    Task LogAsync(
        int? userId,
        string? actorRole,
        string action,
        string? entityType = null,
        int? entityId = null,
        object? oldValue = null,
        object? newValue = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default
    );
}
