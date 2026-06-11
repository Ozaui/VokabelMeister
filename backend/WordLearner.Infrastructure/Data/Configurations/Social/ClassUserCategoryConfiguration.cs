/// <summary>
/// ClassUserCategoryConfiguration.cs
///
/// AMAÇ: ClassUserCategories ara tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Class ↔ Kişisel Kategori M:N; (ClassId, UserCategoryId) UNIQUE olmalı.
/// BAĞIMLILIKLAR: ClassUserCategory entity, Class entity, UserCategory entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class ClassUserCategoryConfiguration : IEntityTypeConfiguration<ClassUserCategory>
{
    public void Configure(EntityTypeBuilder<ClassUserCategory> builder)
    {
        builder.ToTable("ClassUserCategories");

        builder.HasIndex(cuc => new { cuc.ClassId, cuc.UserCategoryId }).IsUnique();
        builder.HasIndex(cuc => cuc.ClassId);
        builder.HasIndex(cuc => cuc.UserCategoryId);

        builder.HasOne(cuc => cuc.UserCategory)
            .WithMany(uc => uc.ClassUserCategories)
            .HasForeignKey(cuc => cuc.UserCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
