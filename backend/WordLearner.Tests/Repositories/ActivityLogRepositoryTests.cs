using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Repositories.Logging;

namespace WordLearner.Tests.Repositories;

public class ActivityLogRepositoryTests
{
    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetPagedAsync_UserIdFilterProvided_ReturnsOnlyThatUsersLogs()
    {
        // ARRANGE
        await using var context = CreateContext();
        context.ActivityLogs.AddRange(
            new ActivityLog { UserId = 1, Action = "LOGIN", CreatedAt = DateTime.UtcNow },
            new ActivityLog { UserId = 2, Action = "LOGIN", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new ActivityLogRepository(context);

        // ACT
        var result = await repository.GetPagedAsync(userId: 1, action: null, entityType: null, from: null, to: null, page: 1, pageSize: 10);

        // ASSERT
        result.Items.Should().ContainSingle().Which.UserId.Should().Be(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_MoreRowsThanPageSize_ReturnsRequestedPageOrderedByNewestFirst()
    {
        // ARRANGE — üç satır, en yeniden en eskiye CreatedAt farklı
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.ActivityLogs.AddRange(
            new ActivityLog { Action = "A", CreatedAt = now.AddMinutes(-2) },
            new ActivityLog { Action = "B", CreatedAt = now.AddMinutes(-1) },
            new ActivityLog { Action = "C", CreatedAt = now });
        await context.SaveChangesAsync();
        var repository = new ActivityLogRepository(context);

        // ACT — sayfa boyutu 2, toplam 3 satır olduğu için ikinci sayfada tek satır kalmalı
        var firstPage = await repository.GetPagedAsync(null, null, null, null, null, page: 1, pageSize: 2);
        var secondPage = await repository.GetPagedAsync(null, null, null, null, null, page: 2, pageSize: 2);

        // ASSERT
        firstPage.TotalCount.Should().Be(3);
        firstPage.Items.Should().HaveCount(2);
        firstPage.Items[0].Action.Should().Be("C"); // en yeni önce
        firstPage.Items[1].Action.Should().Be("B");
        secondPage.Items.Should().ContainSingle().Which.Action.Should().Be("A");
    }
}
