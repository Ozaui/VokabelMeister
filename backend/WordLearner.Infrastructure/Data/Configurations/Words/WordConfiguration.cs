using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.Property(w => w.Text).HasMaxLength(255).IsRequired();

        // Bir WordConcept'in aynı dilde iki satırı olamaz — "eşleşmemiş kavram" tanımı buna dayanır.
        builder.HasIndex(w => new { w.WordConceptId, w.LanguageId }).IsUnique();
        builder.HasIndex(w => new { w.LanguageId, w.Text });

        builder
            .HasOne(w => w.WordConcept)
            .WithMany(c => c.Words)
            .HasForeignKey(w => w.WordConceptId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict — Language sabit/seed veri, yanlışlıkla silinirse altındaki kelimeler CASCADE ile gitmemeli.
        builder
            .HasOne(w => w.Language)
            .WithMany()
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
