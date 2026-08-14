using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Infrastructure.Data;

public class WordLearnerDbContext : DbContext
{
    public WordLearnerDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<QrLoginSession> QrLoginSessions => Set<QrLoginSession>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();
    public DbSet<SecurityLog> SecurityLogs => Set<SecurityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordLearnerDbContext).Assembly);

        // Her BaseEntity türevine otomatik "IsDeleted=false" filtresi uygular — yeni bir entity
        // eklendiğinde Repository'de elle .Where(!IsDeleted) yazmayı UNUTMA riski ortadan kalkar.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(WordLearnerDbContext)
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
