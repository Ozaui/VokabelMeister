using FluentAssertions;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class ActivityLogRepositoryTests
{
    [Fact]
    public async Task AddAsync_ValidLog_PersistsRecord()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new ActivityLogRepository(context);

        // ACT
        await repo.AddAsync(new ActivityLog { UserId = 1, Action = "CREATE_WORD" });

        // ASSERT
        context.ActivityLogs.Should().ContainSingle(a => a.Action == "CREATE_WORD" && a.UserId == 1);
    }

    [Fact]
    public async Task GetPagedAsync_FilterByUserId_ReturnsOnlyMatchingRecords()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new ActivityLogRepository(context);
        await repo.AddAsync(new ActivityLog { UserId = 1, Action = "LOGIN" });
        await repo.AddAsync(new ActivityLog { UserId = 2, Action = "LOGIN" });

        // ACT
        var sonuc = await repo.GetPagedAsync(1, null, null, null, null, 1, 10);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(a => a.UserId == 1);
    }

    [Fact]
    public async Task GetPagedAsync_MoreRecordsThanPageSize_ReturnsCorrectPageAndTotalCount()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new ActivityLogRepository(context);
        for (var i = 0; i < 5; i++)
            await repo.AddAsync(new ActivityLog { Action = $"ACTION_{i}" });

        // ACT
        var sonuc = await repo.GetPagedAsync(null, null, null, null, null, 1, 2);

        // ASSERT
        sonuc.TotalCount.Should().Be(5);
        sonuc.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_NoFilters_OrdersByCreatedAtDescending()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new ActivityLogRepository(context);
        await repo.AddAsync(new ActivityLog { Action = "OLDER", CreatedAt = DateTime.UtcNow.AddMinutes(-10) });
        await repo.AddAsync(new ActivityLog { Action = "NEWER", CreatedAt = DateTime.UtcNow });

        // ACT
        var sonuc = await repo.GetPagedAsync(null, null, null, null, null, 1, 10);

        // ASSERT
        sonuc.Items.Select(a => a.Action).Should().ContainInOrder("NEWER", "OLDER");
    }
}
