using Zausel.Application.Common;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Interfaces.Services;

namespace Zausel.Application.Services;

// EvaluateAndUnlockAsync durum-tabanlı (event-tabanlı DEĞİL) çalışır: her çağrıda TÜM eşikleri
// kullanıcının GÜNCEL UserProgress/UserCardProgress/User durumundan yeniden hesaplar — bu yüzden
// A-10 (learn-system-word) ve A-11 (review-answer/oturum tamamlama) yazıldığında, o Handler'lar da
// kendi mutasyonlarından SONRA AYNI metodu çağırarak rozet kilidini açabilir; ayrı bir event/queue
// sistemi gerekmez. Şu an TEK çağıran ApplyWordLeechActionCommandHandler (Reset akışı).
public class AchievementService : IAchievementService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProgressRepository _userProgressRepository;
    private readonly IUserCardProgressRepository _userCardProgressRepository;
    private readonly IUserAchievementRepository _userAchievementRepository;

    public AchievementService(
        IUserRepository userRepository,
        IUserProgressRepository userProgressRepository,
        IUserCardProgressRepository userCardProgressRepository,
        IUserAchievementRepository userAchievementRepository)
    {
        _userRepository = userRepository;
        _userProgressRepository = userProgressRepository;
        _userCardProgressRepository = userCardProgressRepository;
        _userAchievementRepository = userAchievementRepository;
    }

    public async Task EvaluateAndUnlockAsync(int userId, bool leechRecovered = false, CancellationToken cancellationToken = default)
    {
        var wordSnapshots = await _userProgressRepository.GetSnapshotsAsync(userId, cancellationToken);
        var cardSnapshots = await _userCardProgressRepository.GetSnapshotsAsync(userId, cancellationToken);
        var snapshots = wordSnapshots.Concat(cardSnapshots).ToList();

        var wordCount = snapshots.Count;
        var goodBandCount = snapshots.Count(s => s.Mastery >= 70);
        var hasFirstMastery = snapshots.Any(s => s.CurrentLevel >= 5);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var streakDays = user?.StreakDays ?? 0;

        var qualifiedAchievementIds = new List<int>();

        if (streakDays >= 3) qualifiedAchievementIds.Add(AchievementIds.Streak3);
        if (streakDays >= 7) qualifiedAchievementIds.Add(AchievementIds.Streak7);
        if (streakDays >= 30) qualifiedAchievementIds.Add(AchievementIds.Streak30);

        if (wordCount >= 50) qualifiedAchievementIds.Add(AchievementIds.WordCount50);
        if (wordCount >= 200) qualifiedAchievementIds.Add(AchievementIds.WordCount200);
        if (wordCount >= 500) qualifiedAchievementIds.Add(AchievementIds.WordCount500);

        if (hasFirstMastery) qualifiedAchievementIds.Add(AchievementIds.FirstMastery);
        if (goodBandCount >= 100) qualifiedAchievementIds.Add(AchievementIds.GoodBand100);

        if (leechRecovered) qualifiedAchievementIds.Add(AchievementIds.LeechRecovery);

        // FlawlessSession burada YOK — hatasız oturum bir "oturum tamamlandı" OLAYINA bağlı, LearningSession
        // (A-11) henüz yazılmadığı için tespit edilecek bir veri yok; A-11'in oturum-tamamlama Handler'ı
        // yazılınca bu metoda bir "sessionWasFlawless" parametresi eklenip aynı desenle işlenecek.

        var unlockedAny = false;
        foreach (var achievementId in qualifiedAchievementIds)
        {
            if (await _userAchievementRepository.HasUnlockedAsync(userId, achievementId, cancellationToken))
                continue;

            await _userAchievementRepository.AddAsync(userId, achievementId, cancellationToken);
            unlockedAny = true;
        }

        // Rozet açılışı ActivityLogger'a YAZILMAZ — UserAchievement satırının kendisi (UnlockedAt ile)
        // zaten insert-only bir log, LearningHistory'yle AYNI gerekçe.
        if (unlockedAny)
            await _userAchievementRepository.SaveChangesAsync(cancellationToken);
    }
}
