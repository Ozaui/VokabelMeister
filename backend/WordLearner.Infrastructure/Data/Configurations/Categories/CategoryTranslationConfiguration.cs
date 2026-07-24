using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Infrastructure.Data.Configurations.Categories;

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // LanguageConfiguration.cs'teki HasData ile birebir aynı Id'ler.
    private const int GermanLanguageId = 1;
    private const int TurkishLanguageId = 2;

    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(t => new { t.CategoryId, t.LanguageId }).IsUnique();

        builder
            .HasOne(t => t.Category)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Language sabit/seed veri — yanlışlıkla silinirse CASCADE yerine hata istenir.
        builder
            .HasOne(t => t.Language)
            .WithMany()
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            Seed(1, 1, GermanLanguageId, "Menschen"),
            Seed(2, 1, TurkishLanguageId, "İnsanlar"),
            Seed(3, 2, GermanLanguageId, "Familie"),
            Seed(4, 2, TurkishLanguageId, "Aile"),
            Seed(5, 3, GermanLanguageId, "Essen"),
            Seed(6, 3, TurkishLanguageId, "Yemek"),
            Seed(7, 4, GermanLanguageId, "Haus"),
            Seed(8, 4, TurkishLanguageId, "Ev"),
            Seed(9, 5, GermanLanguageId, "Schule"),
            Seed(10, 5, TurkishLanguageId, "Okul"),
            Seed(11, 6, GermanLanguageId, "Zahlen"),
            Seed(12, 6, TurkishLanguageId, "Sayılar"),
            Seed(13, 7, GermanLanguageId, "Farben"),
            Seed(14, 7, TurkishLanguageId, "Renkler"),
            Seed(15, 8, GermanLanguageId, "Zeit"),
            Seed(16, 8, TurkishLanguageId, "Zaman"),
            Seed(17, 9, GermanLanguageId, "Körperteile"),
            Seed(18, 9, TurkishLanguageId, "Vücut"),
            Seed(19, 10, GermanLanguageId, "Tiere"),
            Seed(20, 10, TurkishLanguageId, "Hayvanlar"),
            Seed(21, 11, GermanLanguageId, "Arbeit"),
            Seed(22, 11, TurkishLanguageId, "İş"),
            Seed(23, 12, GermanLanguageId, "Reisen"),
            Seed(24, 12, TurkishLanguageId, "Seyahat")
        );
    }

    private static CategoryTranslation Seed(int id, int categoryId, int languageId, string name) =>
        new()
        {
            Id = id,
            CategoryId = categoryId,
            LanguageId = languageId,
            Name = name,
            CreatedAt = SeedCreatedAt,
        };
}
