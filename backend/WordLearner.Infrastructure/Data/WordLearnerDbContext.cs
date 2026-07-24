using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Entities.System;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Infrastructure.Data;

public class WordLearnerDbContext : DbContext
{
    public WordLearnerDbContext(DbContextOptions<WordLearnerDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<QrLoginSession> QrLoginSessions => Set<QrLoginSession>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Serilog MSSqlServer sink'i yazar, EF Core yalnızca okur.
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
    public DbSet<SmtpSettings> SmtpSettings => Set<SmtpSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordLearnerDbContext).Assembly);

        // BaseEntity'den türeyen her entity'ye global soft delete filtresi — IgnoreQueryFilters() ile bypass edilir.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(
                    entityType.ClrType,
                    "e"
                );
                var property = System.Linq.Expressions.Expression.Property(
                    parameter,
                    nameof(BaseEntity.IsDeleted)
                );
                var notDeleted = System.Linq.Expressions.Expression.Not(property);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Insert sonrası UpdatedAt null kalır (BaseEntity'de dokunulmuyor) — "hiç güncellenmedi" bilgisini korur.
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
