using Microsoft.EntityFrameworkCore;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Tests.Repositories;

public class TestDbContext(DbContextOptions<TestDbContext> options) : WordLearnerDbContext(options)
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}
