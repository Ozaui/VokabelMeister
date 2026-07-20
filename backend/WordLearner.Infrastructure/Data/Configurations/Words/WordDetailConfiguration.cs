// ─────────────────────────────────────────────────────────────────────────────
// WordDetailConfiguration.cs
//
// AMAÇ: WordDetail entity'sinin EF Core tablo eşlemesini tanımlar (1:1 ile Word).
// NEDEN: WordId üzerinde UNIQUE index — bu ilişkinin gerçekten 1:1 olduğunu DB
//        seviyesinde garanti eder (bir Word'ün iki WordDetail'i olamaz).
// BAĞIMLILIKLAR: EF Core, WordDetail entity, Word entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class WordDetailConfiguration : IEntityTypeConfiguration<WordDetail>
{
    public void Configure(EntityTypeBuilder<WordDetail> builder)
    {
        builder.Property(d => d.Pronunciation).HasMaxLength(500);
        builder.Property(d => d.AudioUrl).HasMaxLength(500);

        builder.HasIndex(d => d.WordId).IsUnique();

        builder
            .HasOne(d => d.Word)
            .WithOne(w => w.WordDetail)
            .HasForeignKey<WordDetail>(d => d.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
