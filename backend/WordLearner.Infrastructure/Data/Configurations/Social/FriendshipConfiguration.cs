/// <summary>
/// FriendshipConfiguration.cs
///
/// AMAÇ: Friendships tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: İki FK → Users olduğundan SQL Server çoklu CASCADE yolunu engeller;
///        her iki FK Restrict olarak tanımlanmalı.
/// BAĞIMLILIKLAR: Friendship entity, User entity (Requester, Receiver)
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Social;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");

        builder.Property(f => f.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Friendships_Status",
                "[Status] IN ('Pending','Accepted','Rejected','Blocked')");
            // Kullanıcı kendisiyle arkadaş olamaz
            t.HasCheckConstraint("CK_Friendships_Self",
                "[RequesterId] <> [ReceiverId]");
        });

        // Aynı çift için yalnızca bir arkadaşlık isteği olabilir
        builder.HasIndex(f => new { f.RequesterId, f.ReceiverId }).IsUnique();
        builder.HasIndex(f => f.RequesterId);
        builder.HasIndex(f => f.ReceiverId);
        builder.HasIndex(f => f.Status);
    }
}
