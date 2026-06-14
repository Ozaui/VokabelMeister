/// <summary>
/// UserProgressConfiguration.cs
///
/// AMAÇ: UserProgress tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: (UserId, WordId) UNIQUE kısıtı ve SRS sorgularını hızlandıran index'ler kritiktir.
///        EasinessFactor SM-2 algoritması için zorunludur.
/// BAĞIMLILIKLAR: UserProgress entity, User entity, Word entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Learning;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("UserProgress");

        // SM-2 değerleri için hassas ondalık — toplam 5 rakam, 2 ondalık
        builder.Property(p => p.Mastery).HasColumnType("decimal(5,2)");
        builder.Property(p => p.SuccessRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.EasinessFactor).HasColumnType("decimal(5,2)").HasDefaultValue(2.5m);

        // Bir kullanıcı bir kelime için yalnızca bir ilerleme kaydı tutabilir
        builder.HasIndex(p => new { p.UserId, p.WordId }).IsUnique();

        // SRS sorgusu: "Bu kullanıcının bugün tekrar edilecek kelimeleri" — en kritik index
        builder.HasIndex(p => new { p.UserId, p.NextReviewAt });
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.WordId);
        builder.HasIndex(p => p.CurrentLevel);
    }
}
