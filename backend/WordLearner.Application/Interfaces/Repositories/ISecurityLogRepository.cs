// ─────────────────────────────────────────────────────────────────────────────
// ISecurityLogRepository.cs
//
// AMAÇ: SecurityLog satırı ekleme + admin panelin filtreli+sayfalı görüntülemesi
//       (`GET /admin/logs/security` — API_ENDPOINTS.md §11.1) için sözleşme.
// NEDEN: IActivityLogRepository ile aynı gerekçeyle IRepository<T>'yi miras almaz.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.Logging.SecurityLog, LogEventType enum, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ISecurityLogRepository
{
    Task AddAsync(SecurityLog log, CancellationToken ct = default);

    // AMAÇ: eventType/ip/tarih aralığına göre filtrelenmiş, sayfalı liste döner.
    Task<PagedResult<SecurityLog>> GetPagedAsync(
        LogEventType? eventType,
        string? ipAddress,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
