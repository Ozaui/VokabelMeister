using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class LearningHistoryConfiguration : IEntityTypeConfiguration<LearningHistory>
{
    public void Configure(EntityTypeBuilder<LearningHistory> builder)
    {
        builder.ToTable("LearningHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionType).HasMaxLength(50);
        builder.Property(x => x.UserResponse).HasMaxLength(500);
        builder.Property(x => x.CorrectResponse).HasMaxLength(500);
        builder.Property(x => x.MasteryBefore).HasPrecision(5, 2);
        builder.Property(x => x.MasteryAfter).HasPrecision(5, 2);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt).IsDescending();

        // Kullanıcı silinince (Cascade) geçmişi de gider — tek cascade yolu bu, WordId'nin
        // NoAction olması bir "multiple cascade paths" çakışması yaratmaz.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Word'ün soft/hard silinmesi geçmiş kaydını etkilemez — bilinçli NoAction.
        builder.HasOne<Word>()
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        // UserCard ve LearningSession entity'leri henüz yok (sırasıyla A-10/A-11'de gelir) —
        // FK constraint'leri o zaman eklenir, şimdilik düz sütun.
    }
}
