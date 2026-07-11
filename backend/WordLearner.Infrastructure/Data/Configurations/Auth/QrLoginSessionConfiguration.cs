// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSessionConfiguration.cs
//
// AMAÇ: QrLoginSession entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: QrTokenHash üzerinde her scan/status isteğinde arama yapılır; ExpiresAt
//        üzerindeki index süresi dolmuş kayıtları ayrıştıran sorguları hızlandırır
//        (temizlik job'ı yok — süre lazy olarak okuma anında yorumlanır, bkz. TASK).
//        UserId nullable olduğu için (henüz taranmamış oturumlarda null) FK CASCADE
//        yerine SET NULL kullanılır — kullanıcı silinse bile oturum kaydı audit için kalır.
// BAĞIMLILIKLAR: EF Core, QrLoginSession entity, QrLoginStatus enum, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class QrLoginSessionConfiguration : IEntityTypeConfiguration<QrLoginSession>
{
    public void Configure(EntityTypeBuilder<QrLoginSession> builder)
    {
        // NEDEN 44: PasswordService.HashToken → SHA-256 (32 byte) → Base64 = sabit 44 karakter.
        builder.Property(q => q.QrTokenHash).HasMaxLength(44).IsRequired();
        builder.Property(q => q.PairingCode).HasMaxLength(4).IsRequired();

        // NEDEN: Enum DB'de okunabilir bir string olarak tutulur (sayısal index'e bağımlı kalmaz).
        builder
            .Property(q => q.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(QrLoginStatus.Pending);

        builder.Property(q => q.RequesterIp).HasMaxLength(45);
        builder.Property(q => q.RequesterDeviceInfo).HasMaxLength(500);

        // NEDEN: Her scan/confirm/deny/status isteğinde token hash'e göre arama yapılır.
        builder.HasIndex(q => q.QrTokenHash);
        // NEDEN: Süresi dolmuş oturumları ayrıştıran sorgular bu index'i kullanır.
        builder.HasIndex(q => q.ExpiresAt);

        builder
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // NEDEN: Status C# tarafında enum olsa da DB'de string tutulduğu için,
        //        geçersiz bir değer yazılmasını DB seviyesinde engelleyen son savunma hattı.
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_QrLoginSessions_Status",
                "Status IN ('Pending','Scanned','Confirmed','Consumed','Denied','Expired')"
            );
        });
    }
}
