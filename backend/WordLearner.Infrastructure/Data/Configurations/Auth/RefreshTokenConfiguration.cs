// ─────────────────────────────────────────────────────────────────────────────
// RefreshTokenConfiguration.cs
//
// AMAÇ: RefreshToken entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: TokenHash/TokenFamily üzerinde arama sık yapılır (refresh/replay tespiti);
//        User silindiğinde token'ların da silinmesi (CASCADE) veri bütünlüğünü korur.
// BAĞIMLILIKLAR: EF Core, RefreshToken entity, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // NEDEN 44: PasswordService.HashToken → SHA-256 (32 byte) → Base64 = sabit 44 karakter.
        builder.Property(r => r.TokenHash).HasMaxLength(44).IsRequired();
        builder.Property(r => r.TokenFamily).HasMaxLength(36).IsRequired();
        builder.Property(r => r.DeviceInfo).HasMaxLength(500);
        builder.Property(r => r.IpAddress).HasMaxLength(45);

        // NEDEN: Her refresh/logout isteğinde token hash'e göre arama yapılır.
        builder.HasIndex(r => r.TokenHash);
        // NEDEN: Replay tespitinde aynı family'deki tüm token'lar tek sorguda bulunur.
        builder.HasIndex(r => r.TokenFamily);

        builder
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
