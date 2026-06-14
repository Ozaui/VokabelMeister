/// <summary>
/// AchievementConfiguration.cs
///
/// AMAÇ: Achievements tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Admin tarafından yönetilen rozet tanımları; Rarity enum benzeri alan kısıtlanmalı.
/// BAĞIMLILIKLAR: Achievement entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Gamification;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements");

        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.Icon).HasMaxLength(255);
        builder.Property(a => a.Rarity).IsRequired().HasMaxLength(20).HasDefaultValue("Common");

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Achievements_Rarity",
                "[Rarity] IN ('Common','Rare','Epic','Legendary')"
            )
        );
    }
}
