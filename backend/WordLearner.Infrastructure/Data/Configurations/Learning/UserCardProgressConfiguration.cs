/// <summary>
/// UserCardProgressConfiguration.cs
///
/// AMAÇ: UserCardProgress tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: UserProgress ile aynı mantık, kişisel kartlar için.
/// BAĞIMLILIKLAR: UserCardProgress entity, User entity, UserCard entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Learning;

public class UserCardProgressConfiguration : IEntityTypeConfiguration<UserCardProgress>
{
    public void Configure(EntityTypeBuilder<UserCardProgress> builder)
    {
        builder.ToTable("UserCardProgress");

        builder.Property(p => p.Mastery).HasColumnType("decimal(5,2)");
        builder.Property(p => p.SuccessRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.EasinessFactor).HasColumnType("decimal(5,2)").HasDefaultValue(2.5m);

        builder.HasIndex(p => new { p.UserId, p.UserCardId }).IsUnique();
        builder.HasIndex(p => new { p.UserId, p.NextReviewAt });
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.UserCardId);
    }
}
