// ─────────────────────────────────────────────────────────────────────────────
// IQrLoginSessionRepository.cs
//
// AMAÇ: QrLoginSession'a özel sorguları (hash ile arama) IRepository<T>'nin genel
//       CRUD'una ekler.
// NEDEN: scan/confirm/deny/status akışlarının HEPSİ oturumu QrTokenHash'e göre
//        bulur — ham token asla saklanmadığı için doğrulama her zaman hash üzerinden yapılır.
// BAĞIMLILIKLAR: IRepository<T>, WordLearner.Domain.Entities.Auth.QrLoginSession.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Interfaces.Repositories;

public interface IQrLoginSessionRepository : IRepository<QrLoginSession>
{
    // AMAÇ: SHA-256 hash'ine göre QR oturum kaydını bulur.
    Task<QrLoginSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
}
