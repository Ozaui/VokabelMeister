using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Application.Common;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Enums.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    // HasData migration-zamanında sabitlenir — DateTime.UtcNow burada KULLANILMAZ, her migration
    // scaffold'unda farklı bir değer üretip sahte bir diff yaratırdı (CategoryConfiguration ile AYNI gerekçe).
    private static readonly DateTime SeedTimestamp = new(2026, 8, 22, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements", tb =>
        {
            tb.HasCheckConstraint("CK_Achievements_Rarity", "Rarity IN ('Common','Rare','Epic','Legendary')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Icon).HasMaxLength(255);
        builder.Property(x => x.RewardXP).HasDefaultValue(0);
        builder.Property(x => x.Rarity).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(AchievementRarity.Common);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Seed — Id'ler AchievementIds sabitleriyle (AchievementService/AchievementMessages) birebir
        // eşleşir (Languages'teki Id=1/Id=2 seed deseniyle AYNI). Name/Description sütunu YOK — rozetin
        // görünen adı/açıklaması CLAUDE.md §1 "istemciye giden mesaj" istisnasına göre AchievementMessages
        // sözlüğünden Accept-Language'a göre çözülür (ErrorMessages/SuccessMessages ile AYNI desen), yeni
        // bir dil migration gerektirmeden yalnızca o sözlüğe eklenir. Icon şimdilik null — rozet görseli
        // ayrı bir task (medya yükleme akışı yalnızca Admin'e açık, A-07).
        builder.HasData(
            new Achievement { Id = AchievementIds.Streak3, RewardXP = 10, Rarity = AchievementRarity.Common, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.Streak7, RewardXP = 25, Rarity = AchievementRarity.Rare, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.Streak30, RewardXP = 50, Rarity = AchievementRarity.Epic, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.WordCount50, RewardXP = 10, Rarity = AchievementRarity.Common, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.WordCount200, RewardXP = 25, Rarity = AchievementRarity.Rare, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.WordCount500, RewardXP = 50, Rarity = AchievementRarity.Epic, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.FirstMastery, RewardXP = 30, Rarity = AchievementRarity.Rare, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.GoodBand100, RewardXP = 40, Rarity = AchievementRarity.Epic, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.FlawlessSession, RewardXP = 20, Rarity = AchievementRarity.Rare, CreatedAt = SeedTimestamp },
            new Achievement { Id = AchievementIds.LeechRecovery, RewardXP = 15, Rarity = AchievementRarity.Common, CreatedAt = SeedTimestamp }
        );
    }
}
