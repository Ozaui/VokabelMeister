using FluentAssertions;
using Moq;
using Zausel.Application.Features.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Tests.Features.Progress;

public class GetProgressWordsQueryHandlerTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();

    private GetProgressWordsQueryHandler CreateHandler() => new(_userProgressRepository.Object);

    [Fact]
    public async Task Handle_WeakBand_QueriesZeroToFortyExclusiveRange()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetByMasteryRangeAsync(1, 0m, 40m, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetProgressWordsQuery(1, "Weak"), default);

        // ASSERT — [0,40) aralığı repository'ye BİREBİR geçirilmeli
        _userProgressRepository.Verify(r => r.GetByMasteryRangeAsync(1, 0m, 40m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GoodBand_QueriesWithNoUpperBound()
    {
        // ARRANGE — "İyi" bandının üst sınırı YOK (Mastery hiçbir zaman 100'ü aşmaz)
        _userProgressRepository.Setup(r => r.GetByMasteryRangeAsync(1, 70m, null, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetProgressWordsQuery(1, "Good"), default);

        // ASSERT
        _userProgressRepository.Verify(r => r.GetByMasteryRangeAsync(1, 70m, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsItems_MapsAllFieldsToResponse()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetByMasteryRangeAsync(1, 40m, 70m, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new WordProgressItem(5, "laufen", "koşmak", 3, 55m, null, false, 1)
        ]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetProgressWordsQuery(1, "Medium"), default);

        // ASSERT
        result.Should().ContainSingle();
        result[0].WordId.Should().Be(5);
        result[0].Text.Should().Be("laufen");
        result[0].Mastery.Should().Be(55m);
    }
}
