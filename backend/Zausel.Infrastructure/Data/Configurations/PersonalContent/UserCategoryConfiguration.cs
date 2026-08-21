using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Infrastructure.Data.Configurations.PersonalContent;

public class UserCategoryConfiguration : IEntityTypeConfiguration<UserCategory>
{
    public void Configure(EntityTypeBuilder<UserCategory> builder)
    {
        builder.ToTable("UserCategories");

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Color).HasMaxLength(10);
        builder.Property(x => x.Icon).HasMaxLength(100);

        builder.HasIndex(x => x.UserId);

        // Cascade — DATABASE_SCHEMA/Kisisel_Icerik.md ile birebir: kullanıcı silinince kişisel
        // kategorileri de gider (UserCards ile AYNI kural).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
