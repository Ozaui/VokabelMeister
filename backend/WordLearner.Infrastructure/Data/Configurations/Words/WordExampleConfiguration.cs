using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class WordExampleConfiguration : IEntityTypeConfiguration<WordExample>
{
    public void Configure(EntityTypeBuilder<WordExample> builder)
    {
        builder.Property(e => e.Level).HasMaxLength(2).IsRequired();
        builder.Property(e => e.ExampleType).HasMaxLength(20).IsRequired();

        builder.HasIndex(e => new { e.WordId, e.Level });

        builder
            .HasOne(e => e.Word)
            .WithMany(w => w.WordExamples)
            .HasForeignKey(e => e.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-ref FK — Restrict, SQL Server'da CASCADE döngü riski taşır.
        builder
            .HasOne(e => e.PairedExample)
            .WithMany()
            .HasForeignKey(e => e.PairedExampleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_WordExamples_Level", "Level IN ('A1','A2','B1','B2','C1','C2')");
            t.HasCheckConstraint(
                "CK_WordExamples_ExampleType",
                "ExampleType IN ('Normal','Idiom','Formal','Colloquial')"
            );
        });
    }
}
