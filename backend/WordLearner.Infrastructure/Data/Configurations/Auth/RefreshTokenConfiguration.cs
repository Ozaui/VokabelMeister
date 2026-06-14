/// <summary>
/// RefreshTokenConfiguration.cs
///
/// AMAÇ: RefreshTokens tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Token güvenliği için alan uzunlukları ve index'ler kritik öneme sahiptir.
/// BAĞIMLILIKLAR: RefreshToken entity, User entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        // SHA-256 hash Base64 çıktısı 88 karakter
        builder.Property(r => r.TokenHash).IsRequired().HasMaxLength(88);

        // Token Family Pattern için GUID — 36 karakter
        builder.Property(r => r.TokenFamily).IsRequired().HasMaxLength(36);

        builder.Property(r => r.DeviceInfo).HasMaxLength(500);
        builder.Property(r => r.IpAddress).HasMaxLength(45);

        // TokenHash ve TokenFamily üzerinde index — sıkça sorgulanır
        builder.HasIndex(r => r.TokenHash);
        builder.HasIndex(r => r.TokenFamily);
        builder.HasIndex(r => r.UserId);
    }
}
