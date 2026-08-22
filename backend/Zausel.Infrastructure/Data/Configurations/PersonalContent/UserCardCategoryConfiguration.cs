using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Infrastructure.Data.Configurations.PersonalContent;

public class UserCardCategoryConfiguration : IEntityTypeConfiguration<UserCardCategory>
{
    public void Configure(EntityTypeBuilder<UserCardCategory> builder)
    {
        builder.ToTable("UserCardCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.UserCardId, x.CategoryId }).IsUnique();

        builder.HasOne<UserCard>()
            .WithMany()
            .HasForeignKey(x => x.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
