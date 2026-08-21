using FluentAssertions;
using Moq;
using Zausel.Application.Features.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Interfaces.Services;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Progress;

public class ApplyWordLeechActionCommandHandlerTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private ApplyWordLeechActionCommandHandler CreateHandler() => new(_userProgressRepository.Object, _activityLogger.Object);

    [Fact]
    public async Task Handle_NotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(1, 99, It.IsAny<CancellationToken>())).ReturnsAsync((UserProgress?)null);
        var command = new ApplyWordLeechActionCommand(99, "Suspend", UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _userProgressRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProgress>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SuspendAction_SetsIsSuspendedAndLogsActivity()
    {
        // ARRANGE
        var existing = new UserProgress { Id = 1, UserId = 5, WordId = 5, IsSuspended = false, ConsecutiveIncorrect = 5 };
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(5, 5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new ApplyWordLeechActionCommand(5, "Suspend", UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT — yalnızca IsSuspended döner, diğer alanlar ETKİLENMEZ (ConsecutiveIncorrect KORUNUR)
        result.IsSuspended.Should().BeTrue();
        result.CurrentLevel.Should().BeNull();
        result.Acknowledged.Should().BeNull();
        existing.IsSuspended.Should().BeTrue();
        existing.ConsecutiveIncorrect.Should().Be(5);
        _userProgressRepository.Verify(r => r.UpdateAsync(existing, 5, It.IsAny<CancellationToken>()), Times.Once);
        _userProgressRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(l => l.LogAsync(
            5, "User", "APPLY_LEECH_ACTION", "UserProgress", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ResetAction_ResetsSm2StateAndPreservesHistoricalStats()
    {
        // ARRANGE — geçmiş istatistikler (TimesCorrect/TimesIncorrect/SuccessRate) KORUNMALI, yalnızca SM-2 durumu sıfırlanmalı
        var existing = new UserProgress
        {
            Id = 1, UserId = 5, WordId = 5, CurrentLevel = 4, EasinessFactor = 1.9m, IntervalDays = 15,
            RepetitionNumber = 6, ConsecutiveIncorrect = 5, IsSuspended = true, NextReviewAt = DateTime.UtcNow,
            TimesCorrect = 12, TimesIncorrect = 6, SuccessRate = 66.67m
        };
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(5, 5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new ApplyWordLeechActionCommand(5, "Reset", UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.CurrentLevel.Should().Be(0);
        result.NextReviewAt.Should().BeNull();
        existing.CurrentLevel.Should().Be(0);
        existing.EasinessFactor.Should().Be(2.5m);
        existing.IntervalDays.Should().Be(1);
        existing.RepetitionNumber.Should().Be(0);
        existing.ConsecutiveIncorrect.Should().Be(0);
        existing.IsSuspended.Should().BeFalse();
        existing.NextReviewAt.Should().BeNull();
        // Mastery, SrsCalculator.CalculateMastery(0, 66.67) ile YENİDEN hesaplanır — SuccessRate'in kendisi DEĞİŞMEZ
        existing.Mastery.Should().Be(SrsCalculator.CalculateMastery(0, 66.67m));
        existing.TimesCorrect.Should().Be(12);
        existing.TimesIncorrect.Should().Be(6);
        existing.SuccessRate.Should().Be(66.67m);
    }

    [Fact]
    public async Task Handle_ContinueAction_DoesNotMutateOrPersistAnything()
    {
        // ARRANGE
        var existing = new UserProgress { Id = 1, UserId = 5, WordId = 5, ConsecutiveIncorrect = 7, IsSuspended = false };
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(5, 5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new ApplyWordLeechActionCommand(5, "Continue", UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT — yalnızca onay döner, HİÇBİR alan değişmez, veritabanına YAZILMAZ
        result.Acknowledged.Should().BeTrue();
        result.IsSuspended.Should().BeNull();
        existing.ConsecutiveIncorrect.Should().Be(7);
        _userProgressRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProgress>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userProgressRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _activityLogger.Verify(l => l.LogAsync(
            It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(),
            It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
