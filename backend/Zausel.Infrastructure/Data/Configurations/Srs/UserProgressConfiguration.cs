using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("UserProgress");

        builder.Property(x => x.Mastery).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(x => x.EasinessFactor).HasPrecision(4, 2).HasDefaultValue(2.5m);
        builder.Property(x => x.SuccessRate).HasPrecision(5, 2).HasDefaultValue(0m);
        builder.Property(x => x.IntervalDays).HasDefaultValue(1);

        // Bir kullanıcı bir kelimeyi yalnızca BİR kez öğrenmeye başlar — ikinci kayıt yerine
        // var olan satır güncellenir.
        builder.HasIndex(x => new { x.UserId, x.WordId }).IsUnique();
        // Due sorgusu (`WHERE UserId=@ ORDER BY NextReviewAt`) bu index'i doğrudan kullanır.
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Word>()
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
