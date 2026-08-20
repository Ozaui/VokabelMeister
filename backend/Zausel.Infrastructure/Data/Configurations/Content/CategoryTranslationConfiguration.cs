using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;

namespace Zausel.Infrastructure.Data.Configurations.Content;

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("CategoryTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.CategoryId, x.LanguageId }).IsUnique();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed — DATABASE_SCHEMA.md ile birebir, LanguageId 1='de', 2='tr'. 12 kategori × 2 dil = 24 satır.
        builder.HasData(
            Seed(1, 1, 1, "Menschen"), Seed(2, 1, 2, "İnsanlar"),
            Seed(3, 2, 1, "Familie"), Seed(4, 2, 2, "Aile"),
            Seed(5, 3, 1, "Essen"), Seed(6, 3, 2, "Yemek"),
            Seed(7, 4, 1, "Haus"), Seed(8, 4, 2, "Ev"),
            Seed(9, 5, 1, "Schule"), Seed(10, 5, 2, "Okul"),
            Seed(11, 6, 1, "Zahlen"), Seed(12, 6, 2, "Sayılar"),
            Seed(13, 7, 1, "Farben"), Seed(14, 7, 2, "Renkler"),
            Seed(15, 8, 1, "Zeit"), Seed(16, 8, 2, "Zaman"),
            Seed(17, 9, 1, "Körperteile"), Seed(18, 9, 2, "Vücut"),
            Seed(19, 10, 1, "Tiere"), Seed(20, 10, 2, "Hayvanlar"),
            Seed(21, 11, 1, "Arbeit"), Seed(22, 11, 2, "İş"),
            Seed(23, 12, 1, "Reisen"), Seed(24, 12, 2, "Seyahat")
        );
    }

    private static CategoryTranslation Seed(int id, int categoryId, int languageId, string name) =>
        new() { Id = id, CategoryId = categoryId, LanguageId = languageId, Name = name };
}
