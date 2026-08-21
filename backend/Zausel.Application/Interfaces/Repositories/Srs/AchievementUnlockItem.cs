namespace Zausel.Application.Interfaces.Repositories.Srs;

// UserAchievementRepository'nin GetUnlockedForUserAsync'inin döndürdüğü şekil — Name/Description
// TAŞIMAZ (Achievement tablosunda o sütunlar yok), çağıran Handler AchievementMessages.Resolve'u
// AchievementId + Accept-Language ile çağırıp görünen metni ayrıca üretir.
public record AchievementUnlockItem(int AchievementId, string? Icon, int RewardXP, string Rarity, DateTime UnlockedAt);
