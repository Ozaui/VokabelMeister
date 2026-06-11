/// <summary>
/// WordExampleConfiguration.cs
///
/// AMAÇ: WordExamples tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Level ve ExampleType için check kısıtlamaları zorunludur.
/// BAĞIMLILIKLAR: WordExample entity, Word entity, User entity (CreatedBy)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Vocabulary;

public class WordExampleConfiguration : IEntityTypeConfiguration<WordExample>
{
    public void Configure(EntityTypeBuilder<WordExample> builder)
    {
        builder.ToTable("WordExamples");

        builder.Property(e => e.SentenceDE).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.SentenceTR).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.Level).IsRequired().HasMaxLength(2).HasDefaultValue("A1");
        builder.Property(e => e.ExampleType).IsRequired().HasMaxLength(20).HasDefaultValue("Normal");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_WordExamples_Level",
                "[Level] IN ('A1','A2','B1','B2','C1','C2')");
            t.HasCheckConstraint("CK_WordExamples_ExampleType",
                "[ExampleType] IN ('Normal','Idiom','Formal','Colloquial')");
        });

        builder.HasIndex(e => e.WordId);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => new { e.WordId, e.Level });

        // Örneği oluşturan kullanıcı silinse bile örnek cümle kalsın
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
