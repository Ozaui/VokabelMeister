/// <summary>
/// LearningHistoryConfiguration.cs
///
/// AMAÇ: LearningHistory tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Append-only tablo; CreatedAt DESC index ile son girişimler hızlı okunur.
///        İkili FK (WordId/UserCardId) kısıtı uygulama katmanında doğrulanır.
/// BAĞIMLILIKLAR: LearningHistory entity, User, Word, UserCard, LearningSession
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Learning;

public class LearningHistoryConfiguration : IEntityTypeConfiguration<LearningHistory>
{
    public void Configure(EntityTypeBuilder<LearningHistory> builder)
    {
        builder.ToTable("LearningHistory");

        builder.Property(h => h.SessionType).HasMaxLength(50);
        builder.Property(h => h.UserResponse).HasMaxLength(500);
        builder.Property(h => h.CorrectResponse).HasMaxLength(500);

        // Son girişimler DESC sırayla listelenir
        builder.HasIndex(h => h.CreatedAt);
        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => h.WordId);
        builder.HasIndex(h => h.UserCardId);
        builder.HasIndex(h => h.LearningSessionId);

        // UserCard ile ilişki — UserCard silinse bile geçmiş kayıt korunur
        builder
            .HasOne(h => h.UserCard)
            .WithMany()
            .HasForeignKey(h => h.UserCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // LearningSession ile ilişki — oturum silinse bile geçmiş kayıt kalsın
        builder
            .HasOne(h => h.LearningSession)
            .WithMany(s => s.LearningHistories)
            .HasForeignKey(h => h.LearningSessionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
