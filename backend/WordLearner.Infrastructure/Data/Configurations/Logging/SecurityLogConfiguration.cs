using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
{
    public void Configure(EntityTypeBuilder<SecurityLog> builder)
    {
        builder.ToTable("SecurityLogs", tb =>
            tb.HasCheckConstraint("CK_SecurityLogs_EventType",
                "EventType IN ('LoginFailed','OtpFailed','RateLimitHit','UnauthorizedAccess','TokenReplay','PasswordReset','AccountDeletion','AdminAction','QrLoginConfirmed','QrLoginDenied')"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(44).IsUnicode(false);
        builder.Property(x => x.IpAddress).HasMaxLength(45).IsUnicode(false);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.IpAddress);
        builder.HasIndex(x => x.CreatedAt).IsDescending();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
