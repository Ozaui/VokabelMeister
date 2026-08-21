using Zausel.Domain.Enums.Srs;

namespace Zausel.Domain.Entities.Srs;

// BaseEntity'den BİLİNÇLİ olarak türemez — statik seed/referans tablosu (streak/kelime sayısı
// eşikleri admin panelden değil kodla seed edilir), audit/soft-delete anlamsız; DATABASE_SCHEMA/
// SRS.md'deki Achievements tablosuyla birebir eşleşir. Name/Description sütunu YOK — dile bağlı
// görünen ad/açıklama CLAUDE.md §1 "istemciye giden mesaj" istisnasına göre AchievementMessages
// sözlüğünden Accept-Language'a göre çözülür (WordConcept/CategoryTranslations'ın DB-satırı-başına-
// dil deseni DEĞİL, ErrorMessages/SuccessMessages'ın kod-sözlüğü deseni — Achievement admin CRUD'u
// olmayan, kodla sabit-seed edilen bir referans tablosu olduğu için o desen daha uygun).
public class Achievement
{
    public int Id { get; set; }
    // Resim URL'i (emoji değil) — CLAUDE.md görsel tasarım kurallarında emoji yasak.
    public string? Icon { get; set; }
    public int RewardXP { get; set; }
    public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
    public DateTime CreatedAt { get; set; }
}
