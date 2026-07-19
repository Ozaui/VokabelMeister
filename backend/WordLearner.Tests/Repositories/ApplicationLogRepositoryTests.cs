// ─────────────────────────────────────────────────────────────────────────────
// ApplicationLogRepositoryTests.cs
//
// AMAÇ: ApplicationLogRepository'nin filtreli/sayfalı sorgusunu gerçek bir
//       in-memory EF Core bağlamına karşı doğrulamak.
// NEDEN: Bu repository'nin Add metodu YOK (satırları Serilog sink'i yazar — bkz.
//        IApplicationLogRepository.cs) — testler satırları doğrudan DbContext
//        üzerinden ekler, tıpkı gerçek ortamda Serilog'un yaptığı gibi.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.ApplicationLogRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class ApplicationLogRepositoryTests
{
    /// <summary>
    /// GetPagedAsync_FilterByLevel_ReturnsOnlyMatchingRecords
    ///
    /// AMAÇ: level filtresi verildiğinde yalnızca o seviyedeki kayıtların
    ///       döndüğünü doğrulamak (ör. yalnızca "Error" loglarını görüntüleme).
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_FilterByLevel_ReturnsOnlyMatchingRecords()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        context.ApplicationLogs.AddRange(
            new ApplicationLog { Level = "Error", Message = "Bir hata oluştu" },
            new ApplicationLog { Level = "Information", Message = "İstek tamamlandı" }
        );
        await context.SaveChangesAsync();
        var repo = new ApplicationLogRepository(context);

        // ACT
        var sonuc = await repo.GetPagedAsync("Error", null, null, null, 1, 10);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(a => a.Level == "Error");
    }

    /// <summary>
    /// GetPagedAsync_SearchGiven_FiltersByMessageContains
    ///
    /// AMAÇ: search verildiğinde yalnızca Message'ı bu metni İÇEREN kayıtların
    ///       döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_SearchGiven_FiltersByMessageContains()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        context.ApplicationLogs.AddRange(
            new ApplicationLog { Level = "Information", Message = "Request finished: GET /health" },
            new ApplicationLog { Level = "Information", Message = "Request finished: POST /auth/login" }
        );
        await context.SaveChangesAsync();
        var repo = new ApplicationLogRepository(context);

        // ACT
        var sonuc = await repo.GetPagedAsync(null, null, null, "/health", 1, 10);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(a => a.Message.Contains("/health"));
    }
}
