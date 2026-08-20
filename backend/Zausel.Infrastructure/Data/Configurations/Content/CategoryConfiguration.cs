using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;

namespace Zausel.Infrastructure.Data.Configurations.Content;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    // HasData migration-zamanında sabitlenir — DateTime.UtcNow burada KULLANILMAZ, her migration
    // scaffold'unda farklı bir değer üretip sahte bir diff yaratırdı.
    private static readonly DateTime SeedTimestamp = new(2026, 8, 18, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", tb =>
        {
            tb.HasCheckConstraint("CK_Categories_MinLevel", "MinLevel IS NULL OR MinLevel IN ('A1','A2','B1','B2','C1','C2')");
            tb.HasCheckConstraint("CK_Categories_MaxLevel", "MaxLevel IS NULL OR MaxLevel IN ('A1','A2','B1','B2','C1','C2')");
        });

        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(10);
        builder.Property(x => x.MinLevel).HasMaxLength(2);
        builder.Property(x => x.MaxLevel).HasMaxLength(2);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);

        builder.HasIndex(x => x.ParentCategoryId);

        // Restrict — çocuğu olan kategori silinemez (A_backend.md A-06 notu, self-ref FK Restrict
        // kısıtıyla tutarlı olacak şekilde bilinçli tercih edildi, Cascade DEĞİL).
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed — DATABASE_SCHEMA.md "Seed Data (başlangıç kategorileri)" ile birebir, 12 kategori.
        builder.HasData(
            Seed(1, 1, "A1", "#FF6B6B", "people"), Seed(2, 2, "A1", "#FF8C42", "family"), Seed(3, 3, "A1", "#95E1D3", "food"),
            Seed(4, 4, "A1", "#4ECDC4", "house"), Seed(5, 5, "A1", "#AA96DA", "school"), Seed(6, 6, "A1", "#FCBAD3", "numbers"),
            Seed(7, 7, "A1", "#A8EDEA", "colors"), Seed(8, 8, "A1", "#FFD89B", "time"), Seed(9, 9, "A1", "#FB7D5B", "body"),
            Seed(10, 10, "A1", "#84DCC6", "animal"), Seed(11, 11, "A2", "#F38181", "work"), Seed(12, 12, "A2", "#C7CEEA", "travel")
        );
    }

    private static Category Seed(int id, int displayOrder, string minLevel, string color, string icon) => new()
    {
        Id = id, DisplayOrder = displayOrder, MinLevel = minLevel, Color = color, Icon = icon, IsActive = true,
        CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp
    };
}
