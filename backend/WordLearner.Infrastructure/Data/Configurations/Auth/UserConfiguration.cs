using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", tb =>
        {
            tb.HasCheckConstraint("CK_Users_Level", "CurrentLevel IN ('A1','A2','B1','B2','C1','C2')");
            tb.HasCheckConstraint("CK_Users_ThemePreference", "ThemePreference IN ('Light','Dark','System')");
            tb.HasCheckConstraint("CK_Users_LanguagePreference", "LanguagePreference IN ('tr','de')");
            tb.HasCheckConstraint("CK_Users_Role", "Role IN ('User','Admin')");
            tb.HasCheckConstraint("CK_Users_AuthProvider", "AuthProvider IN ('Local','Google','Apple')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.PasswordHash).HasMaxLength(60).IsUnicode(false);
        builder.Property(x => x.GoogleId).HasMaxLength(255);
        builder.HasIndex(x => x.GoogleId);
        builder.Property(x => x.AppleId).HasMaxLength(255);
        builder.HasIndex(x => x.AppleId);
        builder.Property(x => x.AuthProvider).HasMaxLength(20).IsRequired().HasDefaultValue("Local");
        builder.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);

        builder.Property(x => x.DailyWordGoal).HasDefaultValue(10);
        builder.Property(x => x.DailyNewWordLimit).HasDefaultValue(5);

        builder.Property(x => x.CurrentLevel).HasMaxLength(2).IsRequired().HasDefaultValue("A1");
        builder.Property(x => x.ThemePreference).HasMaxLength(10).IsRequired().HasDefaultValue("System");
        builder.Property(x => x.LanguagePreference).HasMaxLength(2).IsRequired().HasDefaultValue("tr");
        builder.Property(x => x.TotalXP).HasDefaultValue(0);
        builder.Property(x => x.LifetimeXP).HasDefaultValue(0);
        builder.Property(x => x.StreakDays).HasDefaultValue(0);

        builder.Property(x => x.PendingOtpCodeHash).HasMaxLength(44).IsUnicode(false);
        builder.Property(x => x.PendingOtpCodePurpose).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PendingOtpCodeAttempts).HasDefaultValue(0);

        builder.Property(x => x.IsOnboardingCompleted).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsEmailVerified).HasDefaultValue(false);
        builder.Property(x => x.LastLoginIP).HasMaxLength(45).IsUnicode(false);
        builder.Property(x => x.LoginCount).HasDefaultValue(0);

        builder.Property(x => x.IsAnonymized).HasDefaultValue(false);
        builder.Property(x => x.OriginalEmailHash).HasMaxLength(44).IsUnicode(false);
        builder.Property(x => x.OneSignalPlayerId).HasMaxLength(100);

        builder.Property(x => x.Role).HasMaxLength(20).IsRequired().HasDefaultValue("User");
        builder.HasIndex(x => x.Role);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
