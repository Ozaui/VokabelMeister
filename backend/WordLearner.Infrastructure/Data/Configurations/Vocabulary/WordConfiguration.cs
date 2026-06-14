/// <summary>
/// WordConfiguration.cs
///
/// AMAÇ: Words tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Birden fazla FK → Users ilişkisi olduğundan DeleteBehavior elle belirlenmeli.
/// BAĞIMLILIKLAR: Word entity, User entity (Approver, Creator)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Vocabulary;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");

        builder.Property(w => w.GermanWord).IsRequired().HasMaxLength(255);
        builder.Property(w => w.TurkishTranslation).IsRequired().HasMaxLength(500);
        builder.Property(w => w.PartOfSpeech).IsRequired().HasMaxLength(20);
        builder.Property(w => w.DifficultyLevel).IsRequired().HasMaxLength(2).HasDefaultValue("A1");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Words_Level",
                "[DifficultyLevel] IN ('A1','A2','B1','B2','C1','C2')");
            t.HasCheckConstraint("CK_Words_PartOfSpeech",
                "[PartOfSpeech] IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')");
        });

        // Soft delete — admin "siler" ama kayıt veritabanında kalır
        builder.HasQueryFilter(w => !w.IsDeleted);

        builder.HasIndex(w => w.GermanWord);
        builder.HasIndex(w => w.DifficultyLevel);
        builder.HasIndex(w => w.PartOfSpeech);
        builder.HasIndex(w => w.IsActive);

        // ─── İlişkiler — birden fazla FK → Users, SQL Server çoklu CASCADE engeller ───────
        builder.HasOne(w => w.Approver)
            .WithMany()
            .HasForeignKey(w => w.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Creator)
            .WithMany()
            .HasForeignKey(w => w.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // UpdatedBy için navigation property yok; sadece FK konfigürasyonu
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WordDetail)
            .WithOne(d => d.Word)
            .HasForeignKey<WordDetail>(d => d.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.WordExamples)
            .WithOne(e => e.Word)
            .HasForeignKey(e => e.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.UserProgresses)
            .WithOne(p => p.Word)
            .HasForeignKey(p => p.WordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.LearningHistories)
            .WithOne(h => h.Word)
            .HasForeignKey(h => h.WordId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
