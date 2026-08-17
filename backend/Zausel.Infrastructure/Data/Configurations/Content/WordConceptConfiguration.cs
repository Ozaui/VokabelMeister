using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;

namespace Zausel.Infrastructure.Data.Configurations.Content;

public class WordConceptConfiguration : IEntityTypeConfiguration<WordConcept>
{
    public void Configure(EntityTypeBuilder<WordConcept> builder)
    {
        builder.ToTable("WordConcepts", tb =>
        {
            tb.HasCheckConstraint("CK_WordConcepts_Level", "DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')");
            tb.HasCheckConstraint("CK_WordConcepts_PartOfSpeech",
                "PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Idiom','Other')");
        });

        builder.Property(x => x.PartOfSpeech).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DifficultyLevel).HasMaxLength(2).IsRequired().HasDefaultValue("A1");
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.DifficultyLevel);
    }
}
