using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Enums.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements", tb =>
        {
            tb.HasCheckConstraint("CK_Achievements_Rarity", "Rarity IN ('Common','Rare','Epic','Legendary')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Icon).HasMaxLength(255);
        builder.Property(x => x.RewardXP).HasDefaultValue(0);
        builder.Property(x => x.Rarity).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(AchievementRarity.Common);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
