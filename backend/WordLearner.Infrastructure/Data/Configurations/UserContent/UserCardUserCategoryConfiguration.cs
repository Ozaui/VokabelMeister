/// <summary>
/// UserCardUserCategoryConfiguration.cs
///
/// AMAÇ: UserCardUserCategories ara tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: UserCard ↔ Kişisel Kategori M:N; (UserCardId, UserCategoryId) UNIQUE olmalı.
/// BAĞIMLILIKLAR: UserCardUserCategory entity, UserCard entity, UserCategory entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.UserContent;

public class UserCardUserCategoryConfiguration : IEntityTypeConfiguration<UserCardUserCategory>
{
    public void Configure(EntityTypeBuilder<UserCardUserCategory> builder)
    {
        builder.ToTable("UserCardUserCategories");

        builder.HasIndex(ucuc => new { ucuc.UserCardId, ucuc.UserCategoryId }).IsUnique();
        builder.HasIndex(ucuc => ucuc.UserCardId);
        builder.HasIndex(ucuc => ucuc.UserCategoryId);

        builder.HasOne(ucuc => ucuc.UserCard)
            .WithMany(c => c.UserCardUserCategories)
            .HasForeignKey(ucuc => ucuc.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserCategoryId → NoAction: UserCard→UserCardUserCategory zaten CASCADE ile temizler.
        // İki CASCADE yolu (User→UserCard→UserCardUserCategory ve User→UserCategory→UserCardUserCategory) çakışır.
        builder.HasOne(ucuc => ucuc.UserCategory)
            .WithMany(c => c.UserCardUserCategories)
            .HasForeignKey(ucuc => ucuc.UserCategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
