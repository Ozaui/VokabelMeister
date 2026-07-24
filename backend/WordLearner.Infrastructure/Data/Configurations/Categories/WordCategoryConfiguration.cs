using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Infrastructure.Data.Configurations.Categories;

public class WordCategoryConfiguration : IEntityTypeConfiguration<WordCategory>
{
    public void Configure(EntityTypeBuilder<WordCategory> builder)
    {
        builder.HasIndex(wc => new { wc.WordConceptId, wc.CategoryId }).IsUnique();

        builder
            .HasOne(wc => wc.WordConcept)
            .WithMany(c => c.WordCategories)
            .HasForeignKey(wc => wc.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(wc => wc.Category)
            .WithMany(c => c.WordCategories)
            .HasForeignKey(wc => wc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
