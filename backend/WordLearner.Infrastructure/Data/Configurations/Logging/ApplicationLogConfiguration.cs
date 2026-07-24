using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

// Bu tabloya EF Core DEĞİL Serilog'un MSSqlServer sink'i yazar (AutoCreateSqlTable=false) —
// şema burada ApplicationLogColumnOptions'la BİREBİR eşleşmek zorunda, aksi halde sink INSERT'i kolon uyuşmazlığından patlar.
public class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLog>
{
    public void Configure(EntityTypeBuilder<ApplicationLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();

        builder.Property(a => a.Level).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Message).IsRequired();
        builder.Property(a => a.SourceContext).HasMaxLength(255);
        builder.Property(a => a.RequestPath).HasMaxLength(500);

        builder.HasIndex(a => a.Level);
        builder.HasIndex(a => a.TimeStamp).IsDescending();
    }
}
