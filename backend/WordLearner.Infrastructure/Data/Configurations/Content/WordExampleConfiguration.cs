using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Content;
using WordLearner.Domain.Enums.Content;

namespace WordLearner.Infrastructure.Data.Configurations.Content;

public class WordExampleConfiguration : IEntityTypeConfiguration<WordExample>
{
    public void Configure(EntityTypeBuilder<WordExample> builder)
    {
        builder.ToTable("WordExamples", tb =>
        {
            tb.HasCheckConstraint("CK_WordExamples_Level", "Level IN ('A1','A2','B1','B2','C1','C2')");
            tb.HasCheckConstraint("CK_WordExamples_ExampleType", "ExampleType IN ('Normal','Idiom','Formal','Colloquial')");
        });

        builder.Property(x => x.SentenceText).IsRequired();
        builder.Property(x => x.Level).HasMaxLength(2).IsRequired().HasDefaultValue("A1");
        builder.Property(x => x.ExampleType).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(ExampleType.Normal);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.WordId, x.Level });

        builder.HasOne<Word>()
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-ref — SQL Server self-referencing FK'de CASCADE'e izin vermez, Restrict şart.
        builder.HasOne<WordExample>()
            .WithMany()
            .HasForeignKey(x => x.PairedExampleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
