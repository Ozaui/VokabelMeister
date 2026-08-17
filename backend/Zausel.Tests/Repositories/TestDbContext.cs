using Microsoft.EntityFrameworkCore;
using Zausel.Infrastructure.Data;

namespace Zausel.Tests.Repositories;

public class TestDbContext(DbContextOptions<TestDbContext> options) : ZauselDbContext(options)
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}
