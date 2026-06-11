/// <summary>
/// UserCardConfiguration.cs
///
/// AMAÇ: UserCards tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Soft delete ve sahiplik kontrolü için query filter zorunludur.
/// BAĞIMLILIKLAR: UserCard entity, User entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.UserContent;

public class UserCardConfiguration : IEntityTypeConfiguration<UserCard>
{
    public void Configure(EntityTypeBuilder<UserCard> builder)
    {
        builder.ToTable("UserCards");

        builder.Property(c => c.FrontText).IsRequired().HasMaxLength(500);
        builder.Property(c => c.BackText).IsRequired().HasMaxLength(500);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.AudioUrl).HasMaxLength(500);

        // Soft delete — kullanıcı "siler" ama kayıt veritabanında kalır
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.IsDeleted);

        builder.HasMany(c => c.UserCardExamples)
            .WithOne(e => e.UserCard)
            .HasForeignKey(e => e.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.UserCardProgresses)
            .WithOne(p => p.UserCard)
            .HasForeignKey(p => p.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        // LearningHistory.UserCardId ilişkisi LearningHistoryConfiguration'da yapılandırılır
        // UserCard entity'sinde LearningHistories collection navigation property'si yoktur.
    }
}
