using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;

namespace Zausel.Infrastructure.Data.Configurations.Content;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");

        builder.Property(x => x.Text).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.WordConceptId, x.LanguageId }).IsUnique();
        builder.HasIndex(x => new { x.LanguageId, x.Text });

        builder.HasOne<WordConcept>()
            .WithMany()
            .HasForeignKey(x => x.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
