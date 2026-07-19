// ─────────────────────────────────────────────────────────────────────────────
// SecurityLogConfiguration.cs
//
// AMAÇ: SecurityLog entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: EventType C# tarafında enum ama DB'de okunabilir string tutulur (Status/
//        QrLoginSession ile aynı desen) — sayısal index'e bağımlı kalmaz, DB'de
//        elle sorgu yazan biri (admin/DBA) değeri doğrudan okuyabilir. EmailHash
//        44 karakter (bkz. SecurityLog.cs'teki NEDEN notu — Loglama.md'deki eski
//        VARCHAR(88) burada düzeltildi).
// BAĞIMLILIKLAR: EF Core, SecurityLog entity, LogEventType enum, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
{
    public void Configure(EntityTypeBuilder<SecurityLog> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseIdentityColumn();

        builder.Property(s => s.EventType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(s => s.EmailHash).HasMaxLength(44);
        builder.Property(s => s.IpAddress).HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasMaxLength(500);

        builder.HasIndex(s => s.EventType);
        builder.HasIndex(s => s.IpAddress);
        builder.HasIndex(s => s.CreatedAt).IsDescending();

        builder.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.SetNull);

        // NEDEN: EventType C# tarafında enum olsa da DB'de string tutulduğu için,
        //        geçersiz bir değer yazılmasını DB seviyesinde engelleyen son savunma hattı
        //        (QrLoginSessionConfiguration'daki CK_QrLoginSessions_Status ile aynı desen).
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SecurityLog_EventType",
            "EventType IN ('LoginFailed','OtpFailed','RateLimitHit','UnauthorizedAccess'," +
            "'TokenReplay','PasswordReset','AccountDeletion','AdminAction'," +
            "'QrLoginConfirmed','QrLoginDenied')"));
    }
}
