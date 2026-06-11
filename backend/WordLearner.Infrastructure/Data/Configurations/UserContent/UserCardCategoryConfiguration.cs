/// <summary>
/// UserCardCategoryConfiguration.cs
///
/// AMAÇ: UserCardCategories ara tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: UserCard ↔ Sistem Kategorisi M:N; (UserCardId, CategoryId) UNIQUE olmalı.
/// BAĞIMLILIKLAR: UserCardCategory entity, UserCard entity, Category entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.UserContent;

public class UserCardCategoryConfiguration : IEntityTypeConfiguration<UserCardCategory>
{
    public void Configure(EntityTypeBuilder<UserCardCategory> builder)
    {
        builder.ToTable("UserCardCategories");

        builder.HasIndex(ucc => new { ucc.UserCardId, ucc.CategoryId }).IsUnique();
        builder.HasIndex(ucc => ucc.UserCardId);
        builder.HasIndex(ucc => ucc.CategoryId);

        builder.HasOne(ucc => ucc.UserCard)
            .WithMany(c => c.UserCardCategories)
            .HasForeignKey(ucc => ucc.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sistem kategorisi silinirse bağlantı kalsın (sadece Category soft delete kullanıyor)
        builder.HasOne(ucc => ucc.Category)
            .WithMany(c => c.UserCardCategories)
            .HasForeignKey(ucc => ucc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
