/// <summary>
/// SharedContentImportConfiguration.cs
///
/// AMAÇ: SharedContentImports tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Bir kullanıcı aynı paylaşımı yalnızca bir kez içe aktarabilir — UNIQUE kısıt.
/// BAĞIMLILIKLAR: SharedContentImport entity, SharedContent entity, User entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class SharedContentImportConfiguration : IEntityTypeConfiguration<SharedContentImport>
{
    public void Configure(EntityTypeBuilder<SharedContentImport> builder)
    {
        builder.ToTable("SharedContentImports");

        // Bir kullanıcı aynı içeriği iki kez alamaz
        builder.HasIndex(i => new { i.SharedContentId, i.ImportedByUserId }).IsUnique();
        builder.HasIndex(i => i.SharedContentId);
        builder.HasIndex(i => i.ImportedByUserId);

        builder.HasOne(i => i.ImportedByUser)
            .WithMany()
            .HasForeignKey(i => i.ImportedByUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
