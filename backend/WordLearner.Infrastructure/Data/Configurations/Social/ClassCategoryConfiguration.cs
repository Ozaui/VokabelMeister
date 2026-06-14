/// <summary>
/// ClassCategoryConfiguration.cs
///
/// AMAÇ: ClassCategories ara tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Class ↔ Sistem Kategorisi M:N; (ClassId, CategoryId) UNIQUE olmalı.
/// BAĞIMLILIKLAR: ClassCategory entity, Class entity, Category entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class ClassCategoryConfiguration : IEntityTypeConfiguration<ClassCategory>
{
    public void Configure(EntityTypeBuilder<ClassCategory> builder)
    {
        builder.ToTable("ClassCategories");

        builder.HasIndex(cc => new { cc.ClassId, cc.CategoryId }).IsUnique();
        builder.HasIndex(cc => cc.ClassId);
        builder.HasIndex(cc => cc.CategoryId);

        builder.HasOne(cc => cc.Category)
            .WithMany(c => c.ClassCategories)
            .HasForeignKey(cc => cc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
