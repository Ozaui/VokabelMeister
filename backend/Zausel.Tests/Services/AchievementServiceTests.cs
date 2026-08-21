using FluentAssertions;
using Moq;
using Zausel.Application.Common;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Services;

public class AchievementServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();
    private readonly Mock<IUserCardProgressRepository> _userCardProgressRepository = new();
    private readonly Mock<IUserAchievementRepository> _userAchievementRepository = new();

    private AchievementService CreateService() =>
        new(_userRepository.Object, _userProgressRepository.Object, _userCardProgressRepository.Object, _userAchievementRepository.Object);

    private void SetupEmptyProgress(int userId = 1)
    {
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = userId, StreakDays = 0 });
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    [Fact]
    public async Task Handle_StreakDays3_UnlocksOnlyStreak3()
    {
        // ARRANGE — 3 gün eşiği geçildi ama 7/30 GEÇİLMEDİ, yalnızca Streak3 açılmalı
        SetupEmptyProgress();
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, StreakDays = 3 });
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak3, It.IsAny<CancellationToken>()), Times.Once);
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak7, It.IsAny<CancellationToken>()), Times.Never);
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak30, It.IsAny<CancellationToken>()), Times.Never);
        _userAchievementRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WordAndCardSnapshotsCombinedReach50_UnlocksWordCount50()
    {
        // ARRANGE — UserProgress'ten 30 + UserCardProgress'ten 20 = TOPLAM 50, iki tablo BİRLEŞİK sayılmalı
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, StreakDays = 0 });
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(0, 30).Select(_ => new ProgressSnapshot(10m, null, false)).ToList());
        _userCardProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(0, 20).Select(_ => new ProgressSnapshot(10m, null, false)).ToList());
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.WordCount50, It.IsAny<CancellationToken>()), Times.Once);
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.WordCount200, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AnySnapshotAtCurrentLevel5_UnlocksFirstMastery()
    {
        // ARRANGE
        SetupEmptyProgress();
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProgressSnapshot(100m, null, false, CurrentLevel: 5)]);
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.FirstMastery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_100SnapshotsInGoodBand_UnlocksGoodBand100()
    {
        // ARRANGE — Mastery>=70 olan TAM 100 satır (İyi bant eşiği)
        SetupEmptyProgress();
        _userProgressRepository.Setup(r => r.GetSnapshotsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Range(0, 100).Select(_ => new ProgressSnapshot(70m, null, false)).ToList());
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.GoodBand100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LeechRecoveredTrue_UnlocksLeechRecovery()
    {
        // ARRANGE
        SetupEmptyProgress();
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, leechRecovered: true, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.LeechRecovery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LeechRecoveredFalse_DoesNotUnlockLeechRecovery()
    {
        // ARRANGE
        SetupEmptyProgress();
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.LeechRecovery, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyUnlocked_SkipsReAddingAndReSaving()
    {
        // ARRANGE — Streak3 zaten açık, TEKRAR AddAsync/SaveChangesAsync çağrılmamalı
        SetupEmptyProgress();
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, StreakDays = 3 });
        _userAchievementRepository.Setup(r => r.HasUnlockedAsync(1, AchievementIds.Streak3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak3, It.IsAny<CancellationToken>()), Times.Never);
        _userAchievementRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoQualifyingAchievements_NeverSaves()
    {
        // ARRANGE
        SetupEmptyProgress();
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userAchievementRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleNewAchievements_SavesOnceForAll()
    {
        // ARRANGE — Streak3 VE Streak7 İKİSİ BİRDEN yeni açılıyor, SaveChangesAsync TEK SEFER çağrılmalı
        SetupEmptyProgress();
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 1, StreakDays = 7 });
        var service = CreateService();

        // ACT
        await service.EvaluateAndUnlockAsync(1, cancellationToken: default);

        // ASSERT
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak3, It.IsAny<CancellationToken>()), Times.Once);
        _userAchievementRepository.Verify(r => r.AddAsync(1, AchievementIds.Streak7, It.IsAny<CancellationToken>()), Times.Once);
        _userAchievementRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
