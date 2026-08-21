using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnlockedAt).HasDefaultValueSql("GETUTCDATE()");

        // Aynı başarım bir kullanıcıya iki kez açılmaz.
        builder.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Achievement>()
            .WithMany()
            .HasForeignKey(x => x.AchievementId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
