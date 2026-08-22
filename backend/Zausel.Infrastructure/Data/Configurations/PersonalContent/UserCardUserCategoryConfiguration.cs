using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Infrastructure.Data.Configurations.PersonalContent;

public class UserCardUserCategoryConfiguration : IEntityTypeConfiguration<UserCardUserCategory>
{
    public void Configure(EntityTypeBuilder<UserCardUserCategory> builder)
    {
        builder.ToTable("UserCardUserCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.UserCardId, x.UserCategoryId }).IsUnique();

        builder.HasOne<UserCard>()
            .WithMany()
            .HasForeignKey(x => x.UserCardId)
            .OnDelete(DeleteBehavior.Cascade);

        // NO ACTION — Users silindiğinde hem UserCards hem UserCategories üzerinden CASCADE
        // ulaşılsaydı "multiple cascade paths" hatası olurdu; UserCardId cascade zincirini taşır.
        builder.HasOne<UserCategory>()
            .WithMany()
            .HasForeignKey(x => x.UserCategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
