using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Infrastructure.Data.Configurations.PersonalContent;

public class UserCardConfiguration : IEntityTypeConfiguration<UserCard>
{
    public void Configure(EntityTypeBuilder<UserCard> builder)
    {
        builder.ToTable("UserCards");

        builder.Property(x => x.FrontText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.BackText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.UserId);

        // Cascade — kullanıcı silinince kişisel kartları da gider (UserCategories ile AYNI kural).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
