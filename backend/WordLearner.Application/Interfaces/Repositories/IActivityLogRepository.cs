// ─────────────────────────────────────────────────────────────────────────────
// IActivityLogRepository.cs
//
// AMAÇ: ActivityLog satırı ekleme + admin panelin filtreli+sayfalı görüntülemesi
//       (`GET /admin/logs/activity` — API_ENDPOINTS.md §11.1) için sözleşme.
// NEDEN: IRepository<T>'yi MİRAS ALMAZ — ActivityLog BaseEntity'den türemiyor
//        (log tabloları insert-only, soft delete/update/generic GetById'a ihtiyaç
//        yok — CLAUDE.md "Veri katmanı"). Bu yüzden ayrı, minimal bir sözleşme.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.Logging.ActivityLog, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IActivityLogRepository
{
    // AMAÇ: Yeni bir audit kaydı ekler. Log satırları asla güncellenmediği/silinmediği
    //       için yalnızca Add vardır (Update/Delete yok — insert-only).
    Task AddAsync(ActivityLog log, CancellationToken ct = default);

    // AMAÇ: userId/action/entityType/tarih aralığına göre filtrelenmiş, sayfalı liste döner.
    // NEDEN tüm filtreler opsiyonel (nullable): admin panel hiç filtre uygulamadan da
    //       "son kayıtlar" listesini görebilmeli.
    Task<PagedResult<ActivityLog>> GetPagedAsync(
        int? userId,
        string? action,
        string? entityType,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
