namespace Zausel.Application.Interfaces.Services;

public interface IAchievementService
{
    // leechRecovered yalnızca leech-action Reset akışından true geçirilir — CurrentLevel/Mastery gibi
    // her an sorgulanabilir bir DURUM değil, bir OLAY (Reset anı) olduğu için parametre olarak taşınır.
    Task EvaluateAndUnlockAsync(int userId, bool leechRecovered = false, CancellationToken cancellationToken = default);
}
