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

        // MaxLength yok — NVARCHAR(MAX); Base64(IV+cipher) uzunluğu sabit değil.
        builder.Property(s => s.PasswordEncrypted).IsRequired();
        builder.Property(s => s.FromEmail).IsRequired().HasMaxLength(254);
        builder.Property(s => s.FromName).IsRequired().HasMaxLength(100);
    }
}
