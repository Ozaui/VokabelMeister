// ─────────────────────────────────────────────────────────────────────────────
// UserConfiguration.cs
//
// AMAÇ: User entity'sinin EF Core tablo eşlemesini (kolon uzunlukları, index'ler,
//       CHECK constraint'leri) tanımlar.
// NEDEN: DATABASE_SCHEMA/Auth.md'deki Users şemasıyla birebir eşleşen kısıtları
//        kod tarafında da uygulamak için — ör. Email UNIQUE, Role/CurrentLevel/
//        AuthProvider yalnızca izinli değerleri alabilir.
// BAĞIMLILIKLAR: EF Core, User entity, OtpPurpose enum.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired();
        // NEDEN: Aynı e-posta ile birden fazla hesap açılmasını DB seviyesinde engeller.
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).HasMaxLength(60);
        builder.Property(u => u.GoogleId).HasMaxLength(255);
        builder.Property(u => u.AppleId).HasMaxLength(255);
        builder.Property(u => u.AuthProvider).HasMaxLength(20).HasDefaultValue("Local");

        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);

        builder.Property(u => u.DailyWordGoal).HasDefaultValue(10);
        builder.Property(u => u.DailyNewWordLimit).HasDefaultValue(5);
        builder.Property(u => u.CurrentLevel).HasMaxLength(2).HasDefaultValue("A1");

        builder.Property(u => u.PendingOtpCodeHash).HasMaxLength(88);
        // NEDEN: Enum DB'de okunabilir bir string olarak tutulur (sayısal index'e bağımlı kalmaz).
        builder.Property(u => u.PendingOtpCodePurpose).HasConversion<string>().HasMaxLength(20);

        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.LastLoginIP).HasMaxLength(45);
        builder.Property(u => u.OriginalEmailHash).HasMaxLength(88);
        builder.Property(u => u.OneSignalPlayerId).HasMaxLength(100);
        builder.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");

        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.GoogleId);
        builder.HasIndex(u => u.AppleId);

        // NEDEN: Bu üç alan, C# tarafında enum yerine string tutulduğu için
        //        geçersiz değer yazılmasını DB seviyesinde engelleyen son savunma hattı.
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Users_Level",
                "CurrentLevel IN ('A1','A2','B1','B2','C1','C2')"
            );
            t.HasCheckConstraint("CK_Users_Role", "Role IN ('User','Admin')");
            t.HasCheckConstraint(
                "CK_Users_AuthProvider",
                "AuthProvider IN ('Local','Google','Apple')"
            );
        });
    }
}
