using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Content;

namespace WordLearner.Infrastructure.Data.Configurations.Content;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(5).IsUnicode(false).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NativeName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);

        // Seed — üçüncü dil migration gerektirmez, DB'ye elle satır eklenir (bkz. Icerik.md).
        builder.HasData(
            new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch", IsActive = true, DisplayOrder = 1 },
            new Language { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe", IsActive = true, DisplayOrder = 2 }
        );
    }
}
