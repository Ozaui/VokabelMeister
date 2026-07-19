// ─────────────────────────────────────────────────────────────────────────────
// IApplicationLogRepository.cs
//
// AMAÇ: Admin panelin ApplicationLog görüntülemesi (`GET /admin/logs/application` —
//       API_ENDPOINTS.md §11.1) için salt-okunur, filtreli+sayfalı sözleşme.
// NEDEN: Add YOK — bu tabloya satırları EF Core değil Serilog'un MSSqlServer sink'i
//        yazar (Program.cs, AutoCreateSqlTable=false); Application katmanı bu tabloyu
//        yalnızca OKUR.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.Logging.ApplicationLog, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.Common.Models;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IApplicationLogRepository
{
    // AMAÇ: level/tarih aralığı/serbest metin arama (Message içinde) ile filtrelenmiş,
    //       sayfalı liste döner.
    Task<PagedResult<ApplicationLog>> GetPagedAsync(
        string? level,
        DateTime? from,
        DateTime? to,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default
    );
}
