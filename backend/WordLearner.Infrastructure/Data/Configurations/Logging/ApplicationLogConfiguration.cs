using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

public class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLog>
{
    public void Configure(EntityTypeBuilder<ApplicationLog> builder)
    {
        builder.ToTable("ApplicationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Level).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.SourceContext).HasMaxLength(255);
        builder.Property(x => x.RequestPath).HasMaxLength(500);
        builder.Property(x => x.TimeStamp).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.Level);
        builder.HasIndex(x => x.TimeStamp).IsDescending();

        // FK YOK — Serilog sink User tablosuna join/kontrol yapmaz, ham UserId int'i yazar
        // (DATABASE_SCHEMA/Loglama.md).
    }
}
