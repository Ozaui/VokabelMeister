using FluentAssertions;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class SecurityLogRepositoryTests
{
    [Fact]
    public async Task AddAsync_ValidLog_PersistsRecord()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new SecurityLogRepository(context);

        // ACT
        await repo.AddAsync(new SecurityLog { EventType = LogEventType.LoginFailed, IpAddress = "1.2.3.4" });

        // ASSERT
        context.SecurityLogs.Should().ContainSingle(s => s.EventType == LogEventType.LoginFailed);
    }

    [Fact]
    public async Task GetPagedAsync_FilterByEventType_ReturnsOnlyMatchingRecords()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new SecurityLogRepository(context);
        await repo.AddAsync(new SecurityLog { EventType = LogEventType.LoginFailed });
        await repo.AddAsync(new SecurityLog { EventType = LogEventType.OtpFailed });

        // ACT
        var sonuc = await repo.GetPagedAsync(LogEventType.LoginFailed, null, null, null, 1, 10);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(s => s.EventType == LogEventType.LoginFailed);
    }

    [Fact]
    public async Task GetPagedAsync_FilterByIpAddress_ReturnsOnlyMatchingRecords()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new SecurityLogRepository(context);
        await repo.AddAsync(new SecurityLog { EventType = LogEventType.RateLimitHit, IpAddress = "1.1.1.1" });
        await repo.AddAsync(new SecurityLog { EventType = LogEventType.RateLimitHit, IpAddress = "2.2.2.2" });

        // ACT
        var sonuc = await repo.GetPagedAsync(null, "1.1.1.1", null, null, 1, 10);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(s => s.IpAddress == "1.1.1.1");
    }
}
