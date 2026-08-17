using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Infrastructure.Data.Configurations.Auth;

public class QrLoginSessionConfiguration : IEntityTypeConfiguration<QrLoginSession>
{
    public void Configure(EntityTypeBuilder<QrLoginSession> builder)
    {
        builder.ToTable("QrLoginSessions", tb =>
            tb.HasCheckConstraint("CK_QrLoginSessions_Status", "Status IN ('Pending','Scanned','Confirmed','Consumed','Denied','Expired')"));

        builder.Property(x => x.QrTokenHash).HasMaxLength(44).IsUnicode(false).IsRequired();
        builder.HasIndex(x => x.QrTokenHash);

        builder.Property(x => x.PairingCode).HasMaxLength(4).IsFixedLength().IsUnicode(false).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(QrLoginStatus.Pending);

        builder.Property(x => x.RequesterIp).HasMaxLength(45).IsUnicode(false);
        builder.Property(x => x.RequesterDeviceInfo).HasMaxLength(500);

        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
