using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Infrastructure.Data.Configurations.Categories;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // HasData migration snapshot'ının deterministik olması için sabit — DateTime.UtcNow DEĞİL.
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property(c => c.Color).HasMaxLength(10);
        builder.Property(c => c.MinLevel).HasMaxLength(2);
        builder.Property(c => c.MaxLevel).HasMaxLength(2);

        builder.HasIndex(c => c.ParentCategoryId);

        // Restrict — SQL Server self-referencing FK'de Cascade "çoklu cascade yolu" hatası verir;
        // ayrıca alt kategorili bir üst kategorinin silinmesi zaten DeleteCategoryCommand'da 409'a düşer.
        builder
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Categories_MinLevel", "MinLevel IS NULL OR MinLevel IN ('A1','A2','B1','B2','C1','C2')");
            t.HasCheckConstraint("CK_Categories_MaxLevel", "MaxLevel IS NULL OR MaxLevel IN ('A1','A2','B1','B2','C1','C2')");
        });

        // Id'ler açıkça verilir — HasData IDENTITY otomatik artışına bırakmaz, sabit anahtar üretir.
        builder.HasData(
            Seed(1, 1, "A1", "#FF6B6B", "people"),
            Seed(2, 2, "A1", "#FF8C42", "family"),
            Seed(3, 3, "A1", "#95E1D3", "food"),
            Seed(4, 4, "A1", "#4ECDC4", "house"),
            Seed(5, 5, "A1", "#AA96DA", "school"),
            Seed(6, 6, "A1", "#FCBAD3", "numbers"),
            Seed(7, 7, "A1", "#A8EDEA", "colors"),
            Seed(8, 8, "A1", "#FFD89B", "time"),
            Seed(9, 9, "A1", "#FB7D5B", "body"),
            Seed(10, 10, "A1", "#84DCC6", "animal"),
            Seed(11, 11, "A2", "#F38181", "work"),
            Seed(12, 12, "A2", "#C7CEEA", "travel")
        );
    }

    private static Category Seed(int id, int displayOrder, string minLevel, string color, string icon) =>
        new()
        {
            Id = id,
            DisplayOrder = displayOrder,
            MinLevel = minLevel,
            Color = color,
            Icon = icon,
            IsActive = true,
            CreatedAt = SeedCreatedAt,
        };
}
