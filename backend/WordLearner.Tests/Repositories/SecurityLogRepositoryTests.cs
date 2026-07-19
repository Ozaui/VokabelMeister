// ─────────────────────────────────────────────────────────────────────────────
// SecurityLogRepositoryTests.cs
//
// AMAÇ: SecurityLogRepository'nin ekleme + filtreli/sayfalı sorgusunu gerçek bir
//       in-memory EF Core bağlamına karşı doğrulamak.
// NEDEN: bkz. UserRepositoryTests.cs dosya başı.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.SecurityLogRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class SecurityLogRepositoryTests
{
    /// <summary>
    /// AddAsync_ValidLog_PersistsRecord
    ///
    /// AMAÇ: Mutlu yol — bir güvenlik olayı kaydının eklendiğini doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_FilterByEventType_ReturnsOnlyMatchingRecords
    ///
    /// AMAÇ: eventType filtresi verildiğinde yalnızca o türdeki kayıtların
    ///       döndüğünü doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_FilterByIpAddress_ReturnsOnlyMatchingRecords
    ///
    /// AMAÇ: ipAddress filtresi verildiğinde yalnızca o IP'ye ait kayıtların
    ///       döndüğünü doğrulamak — ör. bir admin belirli bir IP'nin geçmişini araştırırken.
    /// </summary>
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
