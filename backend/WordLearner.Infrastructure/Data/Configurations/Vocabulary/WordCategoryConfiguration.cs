/// <summary>
/// WordCategoryConfiguration.cs
///
/// AMAÇ: WordCategories ara tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Word ↔ Category M:N ilişkisi; (WordId, CategoryId) çifti UNIQUE olmalı.
/// BAĞIMLILIKLAR: WordCategory entity, Word entity, Category entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Vocabulary;

public class WordCategoryConfiguration : IEntityTypeConfiguration<WordCategory>
{
    public void Configure(EntityTypeBuilder<WordCategory> builder)
    {
        builder.ToTable("WordCategories");

        // Bir kelime aynı kategoriye yalnızca bir kez eklenebilir
        builder.HasIndex(wc => new { wc.WordId, wc.CategoryId }).IsUnique();

        builder.HasIndex(wc => wc.CategoryId);
        builder.HasIndex(wc => wc.WordId);

        builder.HasOne(wc => wc.Word)
            .WithMany(w => w.WordCategories)
            .HasForeignKey(wc => wc.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wc => wc.Category)
            .WithMany(c => c.WordCategories)
            .HasForeignKey(wc => wc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
