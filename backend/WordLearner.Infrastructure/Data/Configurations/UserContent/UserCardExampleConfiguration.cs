/// <summary>
/// UserCardExampleConfiguration.cs
///
/// AMAÇ: UserCardExamples tablosunun EF Core Fluent API konfigürasyonu.
/// NEDEN: Kişisel kart silinince örnek cümleler de CASCADE ile silinmelidir.
/// BAĞIMLILIKLAR: UserCardExample entity, UserCard entity
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordLearner.Domain.Entities;

namespace WordLearner.Infrastructure.Data.Configurations.UserContent;

public class UserCardExampleConfiguration : IEntityTypeConfiguration<UserCardExample>
{
    public void Configure(EntityTypeBuilder<UserCardExample> builder)
    {
        builder.ToTable("UserCardExamples");

        builder.Property(e => e.SentenceFront).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.SentenceBack).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(e => e.UserCardId);
    }
}
