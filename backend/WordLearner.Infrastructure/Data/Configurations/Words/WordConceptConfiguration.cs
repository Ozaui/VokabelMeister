using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class WordConceptConfiguration : IEntityTypeConfiguration<WordConcept>
{
    public void Configure(EntityTypeBuilder<WordConcept> builder)
    {
        builder.Property(w => w.PartOfSpeech).HasMaxLength(20).IsRequired();
        builder.Property(w => w.DifficultyLevel).HasMaxLength(2).IsRequired();
        builder.Property(w => w.ImageUrl).HasMaxLength(500);

        builder.HasIndex(w => w.DifficultyLevel);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_WordConcepts_Level",
                "DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')"
            );
            t.HasCheckConstraint(
                "CK_WordConcepts_PartOfSpeech",
                "PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')"
            );
        });
    }
}
