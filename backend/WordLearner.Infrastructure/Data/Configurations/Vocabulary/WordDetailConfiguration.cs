/// <summary>
/// WordDetailConfiguration.cs
///
/// AMAÇ: WordDetails tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: 1:1 ilişki ve çok sayıda opsiyonel alan; ConjugationData JSON için MAX uzunluk.
/// BAĞIMLILIKLAR: WordDetail entity, Word entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Vocabulary;

public class WordDetailConfiguration : IEntityTypeConfiguration<WordDetail>
{
    public void Configure(EntityTypeBuilder<WordDetail> builder)
    {
        builder.ToTable("WordDetails");

        // WordId UNIQUE kısıtı — 1:1 ilişki
        builder.HasIndex(d => d.WordId).IsUnique();

        // Cinsiyet
        builder.Property(d => d.Gender).HasMaxLength(20);

        // Belirli artikeller (4 hâl)
        builder.Property(d => d.ArticleDefiniteNom).HasMaxLength(10);
        builder.Property(d => d.ArticleDefiniteAcc).HasMaxLength(10);
        builder.Property(d => d.ArticleDefiniteDat).HasMaxLength(10);
        builder.Property(d => d.ArticleDefiniteGen).HasMaxLength(10);

        // Belirsiz artikeller (4 hâl)
        builder.Property(d => d.ArticleIndefiniteNom).HasMaxLength(10);
        builder.Property(d => d.ArticleIndefiniteAcc).HasMaxLength(10);
        builder.Property(d => d.ArticleIndefiniteDat).HasMaxLength(10);
        builder.Property(d => d.ArticleIndefiniteGen).HasMaxLength(10);

        // Tekil hâl formları
        builder.Property(d => d.FormNominative).HasMaxLength(255);
        builder.Property(d => d.FormAccusative).HasMaxLength(255);
        builder.Property(d => d.FormDative).HasMaxLength(255);
        builder.Property(d => d.FormGenitive).HasMaxLength(255);

        // Çoğul formlar
        builder.Property(d => d.PluralForm).HasMaxLength(255);
        builder.Property(d => d.PluralFormNominative).HasMaxLength(255);
        builder.Property(d => d.PluralFormAccusative).HasMaxLength(255);
        builder.Property(d => d.PluralFormDative).HasMaxLength(255);
        builder.Property(d => d.PluralFormGenitive).HasMaxLength(255);

        // Fiil çekimi JSON — sınırsız uzunluk
        builder.Property(d => d.ConjugationData).HasColumnType("nvarchar(max)");

        // Ayrılabilir fiil öneki
        builder.Property(d => d.SeparablePrefix).HasMaxLength(50);

        // Telaffuz ve medya
        builder.Property(d => d.Pronunciation).HasMaxLength(500);
        builder.Property(d => d.AudioUrl).HasMaxLength(500);
        builder.Property(d => d.ImageUrl).HasMaxLength(500);

        builder.HasIndex(d => d.Gender);
        builder.HasIndex(d => d.IsSeparableVerb);
    }
}
