using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class UserCardProgressConfiguration : IEntityTypeConfiguration<UserCardProgress>
{
    public void Configure(EntityTypeBuilder<UserCardProgress> builder)
    {
        builder.ToTable("UserCardProgress");

        builder.Property(x => x.Mastery).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(x => x.EasinessFactor).HasPrecision(4, 2).HasDefaultValue(2.5m);
        builder.Property(x => x.SuccessRate).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(x => x.IntervalDays).HasDefaultValue(1);

        builder.HasIndex(x => new { x.UserId, x.UserCardId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserCard entity'si henüz yok (A-10'da gelir) — FK constraint o zaman eklenir, şimdilik düz sütun.
    }
}
