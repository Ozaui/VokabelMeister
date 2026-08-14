using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActorRole).HasMaxLength(20);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(50);
        builder.Property(x => x.IpAddress).HasMaxLength(45).IsUnicode(false);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.CreatedAt).IsDescending();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
