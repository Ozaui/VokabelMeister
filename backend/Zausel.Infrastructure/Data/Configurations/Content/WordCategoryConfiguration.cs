using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;

namespace Zausel.Infrastructure.Data.Configurations.Content;

public class WordCategoryConfiguration : IEntityTypeConfiguration<WordCategory>
{
    public void Configure(EntityTypeBuilder<WordCategory> builder)
    {
        builder.ToTable("WordCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.WordConceptId, x.CategoryId }).IsUnique();

        builder.HasOne<WordConcept>()
            .WithMany()
            .HasForeignKey(x => x.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
