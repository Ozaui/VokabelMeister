/// <summary>
/// LearningSessionConfiguration.cs
///
/// AMAÇ: LearningSessions tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Status check kısıtı ve StartedAt DESC index performans için kritik.
/// BAĞIMLILIKLAR: LearningSession entity, User entity
/// </summary>
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.Learning;

public class LearningSessionConfiguration : IEntityTypeConfiguration<LearningSession>
{
    public void Configure(EntityTypeBuilder<LearningSession> builder)
    {
        builder.ToTable("LearningSessions");

        builder.Property(s => s.SessionType).IsRequired().HasMaxLength(50);
        builder.Property(s => s.SourceType).IsRequired().HasMaxLength(20);
        builder.Property(s => s.LevelFilter).HasMaxLength(2);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(s => s.SuccessRate).HasColumnType("decimal(5,2)");

        // JSON alanlar — NVARCHAR(MAX)
        builder.Property(s => s.CategoryIds).HasColumnType("nvarchar(max)");
        builder.Property(s => s.UserCategoryIds).HasColumnType("nvarchar(max)");

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_LearningSessions_Status",
                "[Status] IN ('Active','Completed','Abandoned')"
            )
        );

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StartedAt);
        builder.HasIndex(s => s.Status);
    }
}
