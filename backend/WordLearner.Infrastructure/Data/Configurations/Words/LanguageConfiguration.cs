using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data.Configurations.Words;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.Property(l => l.Code).HasMaxLength(5).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(50).IsRequired();
        builder.Property(l => l.NativeName).HasMaxLength(50).IsRequired();

        builder.HasIndex(l => l.Code).IsUnique();

        builder.HasData(
            new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch", DisplayOrder = 1 },
            new Language { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe", DisplayOrder = 2 }
        );
    }
}
