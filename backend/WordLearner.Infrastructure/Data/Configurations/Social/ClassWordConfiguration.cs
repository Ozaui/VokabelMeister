/// <summary>
/// ClassWordConfiguration.cs
///
/// AMAÇ: ClassWords tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN:
///   ClassWord, sınıfa özel kelimedir — sistem Words tablosundan tamamen bağımsız.
///   Sınıf silindiğinde ClassWords CASCADE ile silinmelidir (sınıfsız kelime anlamsız).
///   Soft delete query filter: IsDeleted=true olanlar sorgudan otomatik çıkar.
/// BAĞIMLILIKLAR: ClassWord entity, Class entity, User entity (CreatedBy)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class ClassWordConfiguration : IEntityTypeConfiguration<ClassWord>
{
    public void Configure(EntityTypeBuilder<ClassWord> builder)
    {
        builder.ToTable("ClassWords");

        // ─── Zorunlu alanlar ────────────────────────────────────────────────
        builder.Property(cw => cw.GermanWord).IsRequired().HasMaxLength(255);
        builder.Property(cw => cw.TurkishTranslation).IsRequired().HasMaxLength(500);

        // ─── Opsiyonel alanlar ──────────────────────────────────────────────
        builder.Property(cw => cw.PartOfSpeech).HasMaxLength(20);
        builder.Property(cw => cw.Notes).HasMaxLength(4000);
        builder.Property(cw => cw.Gender).HasMaxLength(20);
        builder.Property(cw => cw.ArticleDefiniteNom).HasMaxLength(10);
        builder.Property(cw => cw.PluralForm).HasMaxLength(255);

        // ─── Kısıtlamalar (EF Core 9 ToTable sözdizimi) ──────────────────────
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ClassWords_PartOfSpeech",
                "PartOfSpeech IS NULL OR PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')"
            );
            t.HasCheckConstraint(
                "CK_ClassWords_Gender",
                "Gender IS NULL OR Gender IN ('Masculine','Feminine','Neuter')"
            );
        });

        // ─── Soft delete filtresi ────────────────────────────────────────────
        builder.HasQueryFilter(cw => !cw.IsDeleted);

        // ─── İndeksler ──────────────────────────────────────────────────────
        builder.HasIndex(cw => cw.ClassId);
        builder.HasIndex(cw => cw.CreatedBy);
        builder.HasIndex(cw => cw.IsDeleted);

        // ─── İlişkiler ──────────────────────────────────────────────────────

        // Sınıf silindiğinde sınıfa özel kelimeler de silinir
        builder.HasOne(cw => cw.Class)
            .WithMany(c => c.ClassWords)
            .HasForeignKey(cw => cw.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Instructor silindiğinde kelimeler kalır — admin elle silebilir
        builder.HasOne(cw => cw.CreatedByUser)
            .WithMany()
            .HasForeignKey(cw => cw.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
