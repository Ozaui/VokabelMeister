namespace Zausel.Domain.Entities.Srs;

// BaseEntity'den BİLİNÇLİ olarak türemez — kilit açma insert-only bir olay, soft delete/UpdatedAt
// anlamsız (bir başarım ne geri alınır ne güncellenir); DATABASE_SCHEMA/SRS.md'deki
// UserAchievements tablosuyla birebir eşleşir.
public class UserAchievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }
}
