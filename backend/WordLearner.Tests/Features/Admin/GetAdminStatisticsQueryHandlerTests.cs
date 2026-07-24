// ─────────────────────────────────────────────────────────────────────────────
// GetAdminStatisticsQueryHandlerTests.cs
//
// AMAÇ: GetAdminStatisticsQueryHandler'ın üç repository'den gelen sayaçları doğru
//       birleştirdiğini VE kayıt tarihlerini (ham liste) sıfır-kayıtlı günler dahil
//       eksiksiz bir N-günlük seriye dönüştürdüğünü doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Tests.Features.Admin;

public class GetAdminStatisticsQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();

    private GetAdminStatisticsQueryHandler CreateHandler() =>
        new(_userRepo.Object, _wordConceptRepo.Object, _categoryRepo.Object);

    /// <summary>
    /// Handle_CombinesCountersFromThreeRepositories
    /// </summary>
    [Fact]
    public async Task Handle_CombinesCountersFromThreeRepositories()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((10, 7, 3));
        _wordConceptRepo.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(500);
        _categoryRepo.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(12);
        _userRepo
            .Setup(r => r.GetRegistrationDatesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DateTime>());
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetAdminStatisticsQuery(7), default);

        // ASSERT
        result.TotalUsers.Should().Be(10);
        result.ActiveUsers.Should().Be(7);
        result.FrozenUsers.Should().Be(3);
        result.TotalWordConcepts.Should().Be(500);
        result.TotalCategories.Should().Be(12);
    }

    /// <summary>
    /// Handle_RegistrationDates_FillsZeroCountDaysAndGroupsCorrectly
    ///
    /// AMAÇ: Bazı günlerde hiç kayıt olmasa bile grafiğin `DaysForGraph` uzunluğunda,
    ///       boşluksuz bir seri döndürdüğünü ve aynı güne düşen birden fazla kaydın
    ///       doğru TOPLANDIĞINI doğrulamak.
    /// </summary>
    [Fact]
    public async Task Handle_RegistrationDates_FillsZeroCountDaysAndGroupsCorrectly()
    {
        // ARRANGE
        var today = DateTime.UtcNow.Date;
        _userRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((0, 0, 0));
        _wordConceptRepo.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _categoryRepo.Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _userRepo
            .Setup(r => r.GetRegistrationDatesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DateTime> { today, today.AddHours(5), today.AddDays(-1) });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetAdminStatisticsQuery(3), default);

        // ASSERT — 3 günlük seri: bugün-2, bugün-1, bugün
        result.RegistrationsByDay.Should().HaveCount(3);
        result.RegistrationsByDay.Last().Date.Should().Be(DateOnly.FromDateTime(today));
        result.RegistrationsByDay.Last().Count.Should().Be(2);
        result.RegistrationsByDay[1].Count.Should().Be(1);
        result.RegistrationsByDay[0].Count.Should().Be(0);
    }
}
