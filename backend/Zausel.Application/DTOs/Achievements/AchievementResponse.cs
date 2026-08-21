namespace Zausel.Application.DTOs.Achievements;

public record AchievementResponse(
    int AchievementId, string Name, string Description, string? Icon, int RewardXP, string Rarity, DateTime UnlockedAt);
