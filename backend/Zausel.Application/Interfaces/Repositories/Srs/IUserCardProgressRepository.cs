namespace Zausel.Application.Interfaces.Repositories.Srs;

// Yalnızca GetProgressSummaryQuery'nin bant/due sayımı için — kişisel kart ilerlemesinin metin
// (UserCard.FrontText) gerektiren bant/askıya-alınmış LİSTELEMESİ (UserProgressRepository'deki
// GetByMasteryRangeAsync/GetSuspendedAsync karşılığı) UserCard entity'si henüz olmadığı için
// (A-10'da gelir) buraya EKLENMEDİ — sayım Word'e JOIN gerektirmez, listeleme gerektirir.
public interface IUserCardProgressRepository
{
    Task<List<ProgressSnapshot>> GetSnapshotsAsync(int userId, CancellationToken cancellationToken = default);
}
