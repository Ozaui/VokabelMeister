// ─────────────────────────────────────────────────────────────────────────────
// PagedResult.cs
//
// AMAÇ: Sayfalı sorgu sonuçlarını (öğeler + toplam sayı) taşıyan genel zarf.
// NEDEN: A-02'de YAGNI kuralıyla geri alınmıştı (hiçbir controller/endpoint yokken
//        spekülatif yazılmıştı — bkz. ApiErrorResponse.cs "YAGNI Düzeltmesi"). Şimdi
//        GERÇEK bir tüketicisi var: A-04'ün üç log repository'si (IActivityLogRepository/
//        IApplicationLogRepository/ISecurityLogRepository) `GET /admin/logs/*` (A-07)
//        filtre+sayfa sözleşmesini (API_ENDPOINTS.md §11.1) karşılamak için sayfalı
//        döner — bu yüzden ilk gerçek ihtiyaç doğduğu an (bu task) yazıldı.
// BAĞIMLILIKLAR: Yok — saf generic zarf.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Models;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
