using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Infrastructure.Data.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).HasMaxLength(44).IsUnicode(false).IsRequired();
        builder.HasIndex(x => x.TokenHash);

        builder.Property(x => x.TokenFamily).HasMaxLength(36).IsRequired();
        builder.HasIndex(x => x.TokenFamily);

        builder.Property(x => x.DeviceInfo).HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(45).IsUnicode(false);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
