/// <summary>
/// UserConfiguration.cs
///
/// AMAÇ: Users tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Data annotation yerine Fluent API kullanılır — entity sınıfları temiz kalır.
/// BAĞIMLILIKLAR: User entity, BaseEntity (soft delete query filter)
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // ─── Kimlik Doğrulama ─────────────────────────────────────────────────
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).HasMaxLength(60);
        builder.Property(u => u.GoogleId).HasMaxLength(255);
        builder.Property(u => u.AppleId).HasMaxLength(255);
        builder
            .Property(u => u.AuthProvider)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Local");

        builder.HasIndex(u => u.GoogleId);
        builder.HasIndex(u => u.AppleId);

        // ─── Profil ───────────────────────────────────────────────────────────
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.DisplayName).HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);

        // ─── Dil Tercihleri ───────────────────────────────────────────────────
        builder
            .Property(u => u.PreferredLanguagePair)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("TR-DE");
        builder
            .Property(u => u.PreferredUILanguage)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("tr");

        // ─── Öğrenme Hedefleri ────────────────────────────────────────────────
        builder.Property(u => u.DailyWordGoal).IsRequired().HasDefaultValue(10);
        builder.Property(u => u.DailyNewWordLimit).IsRequired().HasDefaultValue(5);

        // ─── Öğrenme İstatistikleri ───────────────────────────────────────────
        builder.Property(u => u.CurrentLevel).IsRequired().HasMaxLength(2).HasDefaultValue("A1");
        builder.Property(u => u.LastLoginIP).HasMaxLength(45);

        // ─── Rol ──────────────────────────────────────────────────────────────
        builder.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("User");

        // ─── Check Kısıtlamaları ──────────────────────────────────────────────
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Users_Level",
                "[CurrentLevel] IN ('A1','A2','B1','B2','C1','C2')"
            );
            t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('User','Instructor','Admin')");
            t.HasCheckConstraint(
                "CK_Users_AuthProvider",
                "[AuthProvider] IN ('Local','Google','Apple')"
            );
        });

        // ─── Soft Delete Query Filter ─────────────────────────────────────────
        // IsDeleted = true olan kullanıcılar tüm sorgulardan otomatik olarak hariç tutulur
        builder.HasQueryFilter(u => !u.IsDeleted);
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.Role);

        // ─── İlişkiler ────────────────────────────────────────────────────────
        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.UserCards)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.UserCategories)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.UserProgresses)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserCardProgress.UserId → NoAction: UserCard→UserCardProgress zaten CASCADE ile temizler.
        // İki CASCADE yolu (User→UserCard→UserCardProgress ve User→UserCardProgress) SQL Server'da yasak.
        builder
            .HasMany(u => u.UserCardProgresses)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(u => u.LearningHistories)
            .WithOne(h => h.User)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.LearningSessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.UserAchievements)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.ClassMemberships)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Arkadaşlık ilişkisi — SQL Server çoklu CASCADE yolu hatasını önlemek için Restrict
        builder
            .HasMany(u => u.SentFriendships)
            .WithOne(f => f.Requester)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.ReceivedFriendships)
            .WithOne(f => f.Receiver)
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.SharedContents)
            .WithOne(s => s.Owner)
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
