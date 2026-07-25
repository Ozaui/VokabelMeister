using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Services;

public class AccountCleanupServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private AccountCleanupService CreateService() =>
        new(
            _userRepository.Object,
            _passwordService.Object,
            _activityLogger.Object,
            NullLogger<AccountCleanupService>.Instance
        );

    private void SetupExpiredUsers(params User[] users) =>
        _userRepository
            .Setup(r => r.GetPendingAnonymizationAsync(It.IsAny<DateTime>(), default))
            .ReturnsAsync(users);

    private static User CreateDeletedUser(int id = 1) =>
        new()
        {
            Id = id,
            Email = "silinen@example.com",
            FirstName = "Ali",
            LastName = "Veli",
            DisplayName = "aliveli",
            AvatarUrl = "/uploads/avatar.png",
            LastLoginIP = "1.2.3.4",
            OneSignalPlayerId = "player-1",
            PasswordHash = "hash",
            GoogleId = "google-1",
            AppleId = "apple-1",
            IsDeleted = true,
            ScheduledDeletionAt = DateTime.UtcNow.AddDays(-1),
        };

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_ExpiredAccount_ReplacesEmailAndName()
    {
        // ARRANGE
        var user = CreateDeletedUser();
        SetupExpiredUsers(user);
        var service = CreateService();

        // ACT
        await service.AnonymizeExpiredAccountsAsync();

        // ASSERT
        user.Email.Should().Be("deleted_1@deleted.invalid");
        user.FirstName.Should().Be("Silindi");
        user.LastName.Should().Be("Silindi");
        user.IsAnonymized.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_ExpiredAccount_StoresOriginalEmailHashBeforeOverwritingEmail()
    {
        // ARRANGE
        var user = CreateDeletedUser();
        SetupExpiredUsers(user);
        _passwordService.Setup(p => p.HashToken("silinen@example.com")).Returns("email-hash");
        var service = CreateService();

        // ACT
        await service.AnonymizeExpiredAccountsAsync();

        // ASSERT — hash gerçek adresten üretilmezse tekrar kayıt engeli hiçbir zaman eşleşmezdi.
        user.OriginalEmailHash.Should().Be("email-hash");
        _passwordService.Verify(p => p.HashToken("silinen@example.com"), Times.Once);
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_ExpiredAccount_ClearsAllCredentialsAndPersonalData()
    {
        // ARRANGE
        var user = CreateDeletedUser();
        SetupExpiredUsers(user);
        var service = CreateService();

        // ACT
        await service.AnonymizeExpiredAccountsAsync();

        // ASSERT
        user.PasswordHash.Should().BeNull();
        user.GoogleId.Should().BeNull();
        user.AppleId.Should().BeNull();
        user.DisplayName.Should().BeNull();
        user.AvatarUrl.Should().BeNull();
        user.LastLoginIP.Should().BeNull();
        user.OneSignalPlayerId.Should().BeNull();
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_ExpiredAccount_WritesActivityLogWithoutActorRole()
    {
        // ARRANGE
        var user = CreateDeletedUser(42);
        SetupExpiredUsers(user);
        var service = CreateService();

        // ACT
        await service.AnonymizeExpiredAccountsAsync();

        // ASSERT — ActorRole null: bu değişikliği bir kişi değil, zamanlanmış görev yaptı.
        _activityLogger.Verify(
            l => l.LogAsync(
                42,
                null,
                "ANONYMIZE_ACCOUNT",
                "User",
                42,
                null,
                It.IsAny<object>(),
                null,
                null,
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_ExpiredAccount_PersistsWithoutActorUserId()
    {
        // ARRANGE
        var user = CreateDeletedUser();
        SetupExpiredUsers(user);
        var service = CreateService();

        // ACT
        await service.AnonymizeExpiredAccountsAsync();

        // ASSERT — UpdatedByUserId, silinen kullanıcının kendi Id'siyle DOLDURULMAZ; işlemi o yapmadı.
        _userRepository.Verify(r => r.UpdateAsync(user, null, default), Times.Once);
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_MultipleExpiredAccounts_ProcessesEachOne()
    {
        // ARRANGE
        var first = CreateDeletedUser(1);
        var second = CreateDeletedUser(2);
        SetupExpiredUsers(first, second);
        var service = CreateService();

        // ACT
        var count = await service.AnonymizeExpiredAccountsAsync();

        // ASSERT
        count.Should().Be(2);
        first.Email.Should().Be("deleted_1@deleted.invalid");
        second.Email.Should().Be("deleted_2@deleted.invalid");
    }

    [Fact]
    public async Task AnonymizeExpiredAccountsAsync_NoExpiredAccounts_DoesNothing()
    {
        // ARRANGE
        SetupExpiredUsers();
        var service = CreateService();

        // ACT
        var count = await service.AnonymizeExpiredAccountsAsync();

        // ASSERT
        count.Should().Be(0);
        _userRepository.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<int?>(), default),
            Times.Never
        );
        _activityLogger.VerifyNoOtherCalls();
    }
}
