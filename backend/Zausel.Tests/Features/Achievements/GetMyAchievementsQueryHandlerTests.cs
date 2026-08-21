using FluentAssertions;
using Moq;
using Zausel.Application.Common;
using Zausel.Application.Features.Achievements;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Tests.Features.Achievements;

public class GetMyAchievementsQueryHandlerTests
{
    private readonly Mock<IUserAchievementRepository> _userAchievementRepository = new();

    private GetMyAchievementsQueryHandler CreateHandler() => new(_userAchievementRepository.Object);

    [Fact]
    public async Task Handle_NoAcceptLanguage_ResolvesTextInDefaultTurkish()
    {
        // ARRANGE
        var unlockedAt = DateTime.UtcNow;
        _userAchievementRepository.Setup(r => r.GetUnlockedForUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new AchievementUnlockItem(AchievementIds.Streak3, Icon: null, RewardXP: 10, Rarity: "Common", UnlockedAt: unlockedAt)
        ]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetMyAchievementsQuery(1, AcceptLanguage: null), default);

        // ASSERT
        result.Should().HaveCount(1);
        result[0].AchievementId.Should().Be(AchievementIds.Streak3);
        result[0].Name.Should().Be("3 Günlük Seri");
        result[0].RewardXP.Should().Be(10);
        result[0].Rarity.Should().Be("Common");
        result[0].UnlockedAt.Should().Be(unlockedAt);
    }

    [Fact]
    public async Task Handle_GermanAcceptLanguage_ResolvesGermanText()
    {
        // ARRANGE
        _userAchievementRepository.Setup(r => r.GetUnlockedForUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new AchievementUnlockItem(AchievementIds.LeechRecovery, Icon: null, RewardXP: 15, Rarity: "Common", UnlockedAt: DateTime.UtcNow)
        ]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetMyAchievementsQuery(1, AcceptLanguage: "de"), default);

        // ASSERT
        result[0].Name.Should().Be("Leech-Rettung");
    }

    [Fact]
    public async Task Handle_NoUnlockedAchievements_ReturnsEmptyList()
    {
        // ARRANGE
        _userAchievementRepository.Setup(r => r.GetUnlockedForUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetMyAchievementsQuery(1, AcceptLanguage: null), default);

        // ASSERT
        result.Should().BeEmpty();
    }
}
