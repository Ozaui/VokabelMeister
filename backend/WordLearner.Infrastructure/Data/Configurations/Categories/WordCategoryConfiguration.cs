// ─────────────────────────────────────────────────────────────────────────────
// WordCategoryConfiguration.cs
//
// AMAÇ: WordCategory (WordConcept↔Category M:N ara tablosu) entity'sinin EF Core
//       tablo eşlemesi.
// NEDEN: `UQ` niteliğinde `(WordConceptId, CategoryId)` unique index — aynı kavram
//        aynı kategoriye iki kez eklenemez (Icerik.md UQ_WordCategories kısıtı).
//        Hem WordConcept hem Category'ye Cascade — ara tablo satırının tek başına
//        bir anlamı yok, iki taraftan biri silinince bu satırın da gitmesi doğru
//        (WordConfiguration'daki WordConcept→Word Cascade'iyle aynı gerekçe).
// BAĞIMLILIKLAR: EF Core, WordCategory entity, WordConcept entity, Category entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Infrastructure.Data.Configurations.Categories;

public class WordCategoryConfiguration : IEntityTypeConfiguration<WordCategory>
{
    public void Configure(EntityTypeBuilder<WordCategory> builder)
    {
        builder.HasIndex(wc => new { wc.WordConceptId, wc.CategoryId }).IsUnique();

        builder
            .HasOne(wc => wc.WordConcept)
            .WithMany(c => c.WordCategories)
            .HasForeignKey(wc => wc.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(wc => wc.Category)
            .WithMany(c => c.WordCategories)
            .HasForeignKey(wc => wc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
