// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogConfiguration.cs
//
// AMAÇ: ActivityLog entity'sinin EF Core tablo eşlemesini tanımlar.
// NEDEN: UserId/Action/EntityType/CreatedAt üzerinde index — A-07/B-08'in log görüntüleme
//        filtreleri (`GET /admin/logs/activity?userId=&action=&entityType=&from=&to=`) bu
//        kolonlara göre arama yapar. UserId nullable (anonim eylemler) → FK SET NULL
//        (QrLoginSession'daki aynı gerekçe: kullanıcı silinse bile audit kaydı kalmalı).
// BAĞIMLILIKLAR: EF Core, ActivityLog entity, User entity.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data.Configurations.Logging;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();

        builder.Property(a => a.ActorRole).HasMaxLength(20);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(50);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);

        // NEDEN: Filtre kolonlarının her biri ayrı tek-kolon index — birlikte
        //        kullanıldıkları birleşik sorgular (ör. userId+action) henüz
        //        gerçek bir kullanım deseniyle ölçülmedi (YAGNI, tek index yeter).
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.CreatedAt).IsDescending();

        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
