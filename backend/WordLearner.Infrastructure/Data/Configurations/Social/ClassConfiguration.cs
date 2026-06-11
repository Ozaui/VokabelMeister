/// <summary>
/// ClassConfiguration.cs
///
/// AMAÇ: Classes tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: InviteCode UNIQUE olmalı; sınıf sahibi silindiğinde CASCADE değil Restrict.
/// BAĞIMLILIKLAR: Class entity, User entity (Owner)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.InviteCode).IsRequired().HasMaxLength(20);

        // Davet kodu eşsiz olmalı — aynı kod iki sınıfa verilmez
        builder.HasIndex(c => c.InviteCode).IsUnique();

        // Soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.OwnerId);

        // Sınıf sahibi silindiğinde sınıf kalsın — admin elle siler
        builder.HasOne(c => c.Owner)
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.ClassMemberships)
            .WithOne(m => m.Class)
            .HasForeignKey(m => m.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ClassCategories)
            .WithOne(cc => cc.Class)
            .HasForeignKey(cc => cc.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ClassUserCategories)
            .WithOne(cuc => cuc.Class)
            .HasForeignKey(cuc => cuc.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
