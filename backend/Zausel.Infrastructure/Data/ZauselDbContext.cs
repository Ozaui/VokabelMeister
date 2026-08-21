using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Zausel.Domain.Entities;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Logging;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Entities.Srs;

namespace Zausel.Infrastructure.Data;

public class ZauselDbContext : DbContext
{
    public ZauselDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<QrLoginSession> QrLoginSessions => Set<QrLoginSession>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();
    public DbSet<SecurityLog> SecurityLogs => Set<SecurityLog>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<WordConcept> WordConcepts => Set<WordConcept>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<WordDetail> WordDetails => Set<WordDetail>();
    public DbSet<WordExample> WordExamples => Set<WordExample>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<WordCategory> WordCategories => Set<WordCategory>();
    public DbSet<UserCategory> UserCategories => Set<UserCategory>();
    public DbSet<UserProgress> UserProgress => Set<UserProgress>();
    public DbSet<UserCardProgress> UserCardProgress => Set<UserCardProgress>();
    public DbSet<LearningHistory> LearningHistory => Set<LearningHistory>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ZauselDbContext).Assembly);

        // Her BaseEntity türevine otomatik "IsDeleted=false" filtresi uygular — yeni bir entity
        // eklendiğinde Repository'de elle .Where(!IsDeleted) yazmayı UNUTMA riski ortadan kalkar.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(ZauselDbContext)
                .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
            method.Invoke(null, [modelBuilder]);
        }

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
