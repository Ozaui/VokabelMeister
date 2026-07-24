// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettingsConfiguration.cs
//
// AMAÇ: SmtpSettings entity'sinin EF Core tablo eşlemesi.
// NEDEN MaxLength'ler DATABASE_SCHEMA/Sistem.md'deki NVARCHAR uzunluklarıyla
//       birebir: Host/Username 255, FromEmail 254 (RFC 5321 üst sınırı), FromName 100.
//       PasswordEncrypted'e MaxLength verilmez — NVARCHAR(MAX) (Base64 IV+cipher
//       şifreli metnin uzunluğu sabit değil).
// BAĞIMLILIKLAR: EF Core, SmtpSettings entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Infrastructure.Data.Configurations.System;

public class SmtpSettingsConfiguration : IEntityTypeConfiguration<SmtpSettings>
{
    public void Configure(EntityTypeBuilder<SmtpSettings> builder)
    {
        builder.Property(s => s.Host).IsRequired().HasMaxLength(255);
        builder.Property(s => s.Username).IsRequired().HasMaxLength(255);
        builder.Property(s => s.PasswordEncrypted).IsRequired();
        builder.Property(s => s.FromEmail).IsRequired().HasMaxLength(254);
        builder.Property(s => s.FromName).IsRequired().HasMaxLength(100);
    }
}
