using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Repositories.Logging;

namespace WordLearner.Tests.Repositories;

public class SecurityLogRepositoryTests
{
    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetPagedAsync_EventTypeFilterProvided_ReturnsOnlyMatchingEvents()
    {
        // ARRANGE
        await using var context = CreateContext();
        context.SecurityLogs.AddRange(
            new SecurityLog { EventType = LogEventType.LoginFailed, CreatedAt = DateTime.UtcNow },
            new SecurityLog { EventType = LogEventType.TokenReplay, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new SecurityLogRepository(context);

        // ACT
        var result = await repository.GetPagedAsync(LogEventType.LoginFailed, ipAddress: null, from: null, to: null, page: 1, pageSize: 10);

        // ASSERT
        result.Items.Should().ContainSingle().Which.EventType.Should().Be(LogEventType.LoginFailed);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_DateRangeProvided_ExcludesRowsOutsideRange()
    {
        // ARRANGE
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.SecurityLogs.AddRange(
            new SecurityLog { EventType = LogEventType.RateLimitHit, CreatedAt = now.AddDays(-10) }, // aralık dışı
            new SecurityLog { EventType = LogEventType.RateLimitHit, CreatedAt = now });
        await context.SaveChangesAsync();
        var repository = new SecurityLogRepository(context);

        // ACT
        var result = await repository.GetPagedAsync(null, null, from: now.AddDays(-1), to: now.AddDays(1), page: 1, pageSize: 10);

        // ASSERT
        result.TotalCount.Should().Be(1);
    }
}
