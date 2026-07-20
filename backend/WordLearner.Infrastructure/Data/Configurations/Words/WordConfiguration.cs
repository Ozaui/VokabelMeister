// ─────────────────────────────────────────────────────────────────────────────
// WordConfiguration.cs
//
// AMAÇ: Word entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: UQ_Words_Concept_Language — bir WordConcept'in aynı dilde birden fazla
//        satırı olamaz (Icerik.md'nin "eşleşmemiş kavram" tanımı bu kısıtın
//        varlığına dayanır: COUNT(DISTINCT LanguageId)=1 ⇒ eşleşmemiş).
//        Language FK Restrict — Language satırları seed/sabit olduğu için silinmesi
//        beklenmez, yanlışlıkla silinirse altındaki tüm kelimelerin CASCADE ile
//        silinmesi yerine hata vermesi tercih edilir.
// BAĞIMLILIKLAR: EF Core, Word entity, WordConcept entity, Language entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.Property(w => w.Text).HasMaxLength(255).IsRequired();

        builder.HasIndex(w => new { w.WordConceptId, w.LanguageId }).IsUnique();
        builder.HasIndex(w => new { w.LanguageId, w.Text });

        builder
            .HasOne(w => w.WordConcept)
            .WithMany(c => c.Words)
            .HasForeignKey(w => w.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(w => w.Language)
            .WithMany()
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
