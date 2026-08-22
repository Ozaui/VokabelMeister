using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Enums.Srs;

namespace Zausel.Infrastructure.Data.Configurations.Srs;

public class LearningSessionConfiguration : IEntityTypeConfiguration<LearningSession>
{
    public void Configure(EntityTypeBuilder<LearningSession> builder)
    {
        builder.ToTable("LearningSessions", tb =>
        {
            tb.HasCheckConstraint("CK_LearningSessions_SessionType",
                "SessionType IN ('Flashcard','MultipleChoice','ArticleQuiz','PluralQuiz','TranslationQuiz','TrueFalse')");
            tb.HasCheckConstraint("CK_LearningSessions_SourceType", "SourceType IN ('SystemWords','UserCards','Mixed')");
            tb.HasCheckConstraint("CK_LearningSessions_LevelFilter", "LevelFilter IS NULL OR LevelFilter IN ('A1','A2','B1','B2','C1','C2')");
            tb.HasCheckConstraint("CK_LearningSessions_Status", "Status IN ('Active','Completed','Abandoned')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.LevelFilter).HasMaxLength(2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(LearningSessionStatus.Active);

        builder.Property(x => x.StartedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.TotalWords).HasDefaultValue(0);
        builder.Property(x => x.CorrectAnswers).HasDefaultValue(0);
        builder.Property(x => x.IncorrectAnswers).HasDefaultValue(0);
        builder.Property(x => x.SuccessRate).HasPrecision(5, 2);
        builder.Property(x => x.XPEarned).HasDefaultValue(0);

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Word'ün Language FK'ıyla AYNI bilinçli Restrict — bir dil kaydı, ona bağlı oturum geçmişi
        // varken silinemez.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(x => x.TargetLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
