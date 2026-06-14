/// <summary>
/// AuditLogConfiguration.cs
///
/// AMAÇ: AuditLog tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Id tipi long (BIGINT); kullanıcı silinince FK NULL olmalı (SET NULL).
///        Append-only tablo — CreatedAt DESC index ile son olaylar hızlı okunur.
/// BAĞIMLILIKLAR: AuditLog entity, User entity (opsiyonel)
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Audit;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");

        // Id BIGINT — yüksek hacimli kayıt için
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.TableName).HasMaxLength(50);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.Status).HasMaxLength(20);

        // CreatedAt DESC — son olaylar önce listelenir
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);

        // Kullanıcı silinince AuditLog kaydı korunur ama UserId NULL olur
        builder
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
