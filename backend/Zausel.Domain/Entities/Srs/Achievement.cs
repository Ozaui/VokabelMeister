using Zausel.Domain.Enums.Srs;

namespace Zausel.Domain.Entities.Srs;

// BaseEntity'den BİLİNÇLİ olarak türemez — statik seed/referans tablosu (streak/kelime sayısı
// eşikleri admin panelden değil kodla seed edilir), audit/soft-delete anlamsız; DATABASE_SCHEMA/
// SRS.md'deki Achievements tablosuyla birebir eşleşir.
public class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Resim URL'i (emoji değil) — CLAUDE.md görsel tasarım kurallarında emoji yasak.
    public string? Icon { get; set; }
    public int RewardXP { get; set; }
    public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
    public DateTime CreatedAt { get; set; }
}
