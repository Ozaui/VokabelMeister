// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogRepositoryTests.cs
//
// AMAÇ: ActivityLogRepository'nin ekleme + filtreli/sayfalı sorgusunu gerçek bir
//       in-memory EF Core bağlamına karşı doğrulamak.
// NEDEN: bkz. UserRepositoryTests.cs dosya başı.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.ActivityLogRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class ActivityLogRepositoryTests
{
    /// <summary>
    /// AddAsync_ValidLog_PersistsRecord
    ///
    /// AMAÇ: Mutlu yol — bir audit kaydının eklendiğini doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_FilterByUserId_ReturnsOnlyMatchingRecords
    ///
    /// AMAÇ: userId filtresi verildiğinde yalnızca o kullanıcıya ait kayıtların
    ///       döndüğünü doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_MoreRecordsThanPageSize_ReturnsCorrectPageAndTotalCount
    ///
    /// AMAÇ: Sayfa boyutundan fazla kayıt varken doğru sayfanın döndüğünü ve
    ///       TotalCount'un TÜM (filtrelenmiş) kayıt sayısını (sayfa boyutu değil)
    ///       yansıttığını doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_NoFilters_OrdersByCreatedAtDescending
    ///
    /// AMAÇ: Filtre verilmediğinde kayıtların en yeniden en eskiye sıralı döndüğünü
    ///       doğrulamak — admin panel her zaman "son olaylar" görmek ister.
    /// </summary>
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
