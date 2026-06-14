/// <summary>
/// SharedContentConfiguration.cs
///
/// AMAÇ: SharedContents tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: ShareToken UUID benzersiz olmalı; ContentType check kısıtı ile sınırlanmalı.
/// BAĞIMLILIKLAR: SharedContent entity, User entity (Owner)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class SharedContentConfiguration : IEntityTypeConfiguration<SharedContent>
{
    public void Configure(EntityTypeBuilder<SharedContent> builder)
    {
        builder.ToTable("SharedContents");

        // UUID formatında token — paylaşım linkinde kullanılır
        builder.Property(s => s.ShareToken).IsRequired().HasMaxLength(36);
        builder.Property(s => s.ContentType).IsRequired().HasMaxLength(30);

        builder.HasIndex(s => s.ShareToken).IsUnique();
        builder.HasIndex(s => s.OwnerId);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_SharedContents_ContentType",
                "[ContentType] IN ('UserCard','UserCategory','Class')")
        );

        builder.HasMany(s => s.SharedContentImports)
            .WithOne(i => i.SharedContent)
            .HasForeignKey(i => i.SharedContentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
