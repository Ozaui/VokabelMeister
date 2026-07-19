// ─────────────────────────────────────────────────────────────────────────────
// ApplicationLogConfiguration.cs
//
// AMAÇ: ApplicationLog entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: Bu tabloya satırları EF Core DEĞİL, Serilog'un MSSqlServer sink'i yazar
//        (Program.cs, AutoCreateSqlTable=false) — bu yüzden migration'ın ürettiği
//        şema Serilog'un ColumnOptions yapılandırmasıyla (ApplicationLogColumnOptions,
//        A-04.2) BİREBİR eşleşmek zorunda, aksi halde sink INSERT'leri kolon
//        uyuşmazlığından başarısız olur. FK yok (schema'da da yok) — Serilog sink'i
//        User tablosuna join/kontrol yapmaz, yalnızca ham UserId int'i yazar.
// BAĞIMLILIKLAR: EF Core, ApplicationLog entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

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
