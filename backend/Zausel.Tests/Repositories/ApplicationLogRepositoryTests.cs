using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Zausel.Domain.Entities.Logging;
using Zausel.Infrastructure.Repositories.Logging;

namespace Zausel.Tests.Repositories;

public class ApplicationLogRepositoryTests
{
    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetPagedAsync_SearchProvided_MatchesMessageSubstring()
    {
        // ARRANGE
        await using var context = CreateContext();
        context.ApplicationLogs.AddRange(
            new ApplicationLog { Level = "Error", Message = "Database connection failed", TimeStamp = DateTime.UtcNow },
            new ApplicationLog { Level = "Information", Message = "Request completed", TimeStamp = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new ApplicationLogRepository(context);

        // ACT
        var result = await repository.GetPagedAsync(level: null, from: null, to: null, search: "connection", page: 1, pageSize: 10);

        // ASSERT
        result.Items.Should().ContainSingle().Which.Message.Should().Contain("connection");
    }

    [Fact]
    public async Task GetPagedAsync_LevelFilterProvided_ReturnsOnlyThatLevel()
    {
        // ARRANGE
        await using var context = CreateContext();
        context.ApplicationLogs.AddRange(
            new ApplicationLog { Level = "Error", Message = "x", TimeStamp = DateTime.UtcNow },
            new ApplicationLog { Level = "Warning", Message = "y", TimeStamp = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new ApplicationLogRepository(context);

        // ACT
        var result = await repository.GetPagedAsync(level: "Error", from: null, to: null, search: null, page: 1, pageSize: 10);

        // ASSERT
        result.Items.Should().ContainSingle().Which.Level.Should().Be("Error");
    }
}
