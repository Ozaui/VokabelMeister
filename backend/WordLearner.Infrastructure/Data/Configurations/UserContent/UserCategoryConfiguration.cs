/// <summary>
/// UserCategoryConfiguration.cs
///
/// AMAÇ: UserCategories tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Soft delete ve sahiplik query filter için.
/// BAĞIMLILIKLAR: UserCategory entity, User entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.UserContent;

public class UserCategoryConfiguration : IEntityTypeConfiguration<UserCategory>
{
    public void Configure(EntityTypeBuilder<UserCategory> builder)
    {
        builder.ToTable("UserCategories");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Color).HasMaxLength(10);
        builder.Property(c => c.Icon).HasMaxLength(100);

        // Soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.UserId);
    }
}
