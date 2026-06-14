/// <summary>
/// CategoryConfiguration.cs
///
/// AMAÇ: Categories tablosunun EF Core Fluent API konfigürasyonu ve başlangıç seed verisi.
/// NEDEN: Self-referencing FK ve soft delete filter dikkatli ayarlanmalı;
///        seed data ile uygulama sıfırdan başlatıldığında kategoriler hazır olur.
/// BAĞIMLILIKLAR: Category entity (self-referencing)
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Vocabulary;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.Property(c => c.NameDE).IsRequired().HasMaxLength(100);
        builder.Property(c => c.NameTR).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property(c => c.Color).HasMaxLength(10);
        builder.Property(c => c.MinLevel).HasMaxLength(2);
        builder.Property(c => c.MaxLevel).HasMaxLength(2);

        // Soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.ParentCategoryId);

        // Self-referencing: alt kategoriler silinince CASCADE değil Restrict — kaza silmeyi önler
        builder
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Başlangıç Seed Verisi ────────────────────────────────────────────
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            Seed(1, "Menschen", "İnsanlar", 1, "A1", "#FF6B6B", "people", now),
            Seed(2, "Familie", "Aile", 2, "A1", "#FF8C42", "family", now),
            Seed(3, "Essen", "Yemek", 3, "A1", "#95E1D3", "food", now),
            Seed(4, "Haus", "Ev", 4, "A1", "#4ECDC4", "house", now),
            Seed(5, "Schule", "Okul", 5, "A1", "#AA96DA", "school", now),
            Seed(6, "Zahlen", "Sayılar", 6, "A1", "#FCBAD3", "numbers", now),
            Seed(7, "Farben", "Renkler", 7, "A1", "#A8EDEA", "colors", now),
            Seed(8, "Zeit", "Zaman", 8, "A1", "#FFD89B", "time", now),
            Seed(9, "Körperteile", "Vücut Bölümleri", 9, "A1", "#FB7D5B", "body", now),
            Seed(10, "Tiere", "Hayvanlar", 10, "A1", "#84DCC6", "animal", now),
            Seed(11, "Arbeit", "İş", 11, "A2", "#F38181", "work", now),
            Seed(12, "Reisen", "Seyahat", 12, "A2", "#C7CEEA", "travel", now)
        );
    }

    private static Category Seed(
        int id,
        string nameDE,
        string nameTR,
        int order,
        string minLevel,
        string color,
        string icon,
        DateTime now
    ) =>
        new()
        {
            Id = id,
            NameDE = nameDE,
            NameTR = nameTR,
            DisplayOrder = order,
            MinLevel = minLevel,
            Color = color,
            Icon = icon,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };
}
