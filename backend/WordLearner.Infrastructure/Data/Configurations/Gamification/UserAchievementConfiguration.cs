/// <summary>
/// UserAchievementConfiguration.cs
///
/// AMAÇ: UserAchievements tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Bir kullanıcı aynı rozeti yalnızca bir kez kazanabilir — UNIQUE kısıt zorunlu.
/// BAĞIMLILIKLAR: UserAchievement entity, User entity, Achievement entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Gamification;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");

        // Bir kullanıcı aynı rozeti iki kez kazanamaz
        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();
        builder.HasIndex(ua => ua.UserId);

        builder
            .HasOne(ua => ua.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
