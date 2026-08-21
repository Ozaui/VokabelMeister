using FluentAssertions;
using Moq;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Features.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Tests.Features.Progress;

public class GetProgressSummaryQueryHandlerTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();
    private readonly Mock<IUserCardProgressRepository> _userCardProgressRepository = new();

    private GetProgressSummaryQueryHandler CreateHandler() => new(_userProgressRepository.Object, _userCardProgressRepository.Object);

    [Fact]
    public async Task Handle_MixesWordAndCardSnapshots_ReturnsCombinedBandCounts()
    {
        // ARRANGE — UserProgress'ten 1 weak + 1 medium, UserCardProgress'ten 1 good — İKİ tablo TEK sayımda birleşmeli
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ProgressSnapshot(30m, null, false),
            new ProgressSnapshot(55m, null, false)
        ]);
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ProgressSnapshot(85m, null, false)
        ]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetProgressSummaryQuery(1), default);

        // ASSERT
        result.Weak.Should().Be(1);
        result.Medium.Should().Be(1);
        result.Good.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SuspendedItemPastDue_ExcludedFromDueCount()
    {
        // ARRANGE — NextReviewAt geçmişte ama IsSuspended=true — due sayımına GİRMEMELİ
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ProgressSnapshot(20m, DateTime.UtcNow.AddDays(-1), IsSuspended: true)
        ]);
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetProgressSummaryQuery(1), default);

        // ASSERT — bant sayımına (weak) GİRER ama due sayımına GİRMEZ
        result.Weak.Should().Be(1);
        result.DueNow.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NotSuspendedPastDue_IncludedInDueCount()
    {
        // ARRANGE — NextReviewAt geçmişte VE askıya alınmamış — due sayımına GİRMELİ
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new ProgressSnapshot(50m, DateTime.UtcNow.AddDays(-1), IsSuspended: false)
        ]);
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetProgressSummaryQuery(1), default);

        // ASSERT
        result.DueNow.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoSnapshots_ReturnsAllZero()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetProgressSummaryQuery(1), default);

        // ASSERT
        result.Should().Be(new ProgressSummaryResponse(0, 0, 0, 0));
    }
}
