/// <summary>
/// ClassMembershipConfiguration.cs
///
/// AMAÇ: ClassMemberships tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Bir kullanıcı aynı sınıfa iki kez üye olamaz — UNIQUE kısıt zorunlu.
/// BAĞIMLILIKLAR: ClassMembership entity, Class entity, User entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class ClassMembershipConfiguration : IEntityTypeConfiguration<ClassMembership>
{
    public void Configure(EntityTypeBuilder<ClassMembership> builder)
    {
        builder.ToTable("ClassMemberships");

        builder.Property(m => m.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Student");

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_ClassMemberships_Role",
                "[Role] IN ('Student','Teacher')")
        );

        // Bir kullanıcı bir sınıfa yalnızca bir kez üye olabilir
        builder.HasIndex(m => new { m.ClassId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.ClassId);
        builder.HasIndex(m => m.UserId);
    }
}
