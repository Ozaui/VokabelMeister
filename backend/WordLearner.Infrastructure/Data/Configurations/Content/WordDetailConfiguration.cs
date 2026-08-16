using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Content;

namespace WordLearner.Infrastructure.Data.Configurations.Content;

public class WordDetailConfiguration : IEntityTypeConfiguration<WordDetail>
{
    public void Configure(EntityTypeBuilder<WordDetail> builder)
    {
        builder.ToTable("WordDetails");

        builder.Property(x => x.Pronunciation).HasMaxLength(500);

        builder.HasIndex(x => x.WordId).IsUnique();

        builder.HasOne<Word>()
            .WithOne()
            .HasForeignKey<WordDetail>(x => x.WordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
