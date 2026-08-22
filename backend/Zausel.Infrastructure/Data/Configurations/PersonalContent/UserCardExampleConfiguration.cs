using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Infrastructure.Data.Configurations.PersonalContent;

public class UserCardExampleConfiguration : IEntityTypeConfiguration<UserCardExample>
{
    public void Configure(EntityTypeBuilder<UserCardExample> builder)
    {
        builder.ToTable("UserCardExamples");

        builder.Property(x => x.SentenceFront).IsRequired();
        builder.Property(x => x.SentenceBack).IsRequired();
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.UserCardId);

        builder.HasOne<UserCard>()
            .WithMany()
            .HasForeignKey(x => x.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
