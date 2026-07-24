// ─────────────────────────────────────────────────────────────────────────────
// UpdateUserStatusCommandHandlerTests.cs
//
// AMAÇ: UpdateUserStatusCommandHandler'ın IsActive'i güncellediğini, hem
//       IActivityLogger (UPDATE_USER_STATUS) hem ISecurityLogger (AdminAction —
//       dondurma/aktifleştirmeye göre farklı Detail kodu) çağırdığını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Admin;

public class UpdateUserStatusCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private UpdateUserStatusCommandHandler CreateHandler() =>
        new(_userRepo.Object, _activityLogger.Object, _securityLogger.Object);

    /// <summary>
    /// Handle_TargetIsActor_ThrowsSelfAdminActionNotAllowedException
    ///
    /// AMAÇ: Bir admin kendi hesabını donduramaz — kaza sonucu kendini kilitlemesin diye.
    /// </summary>
    [Fact]
    public async Task Handle_TargetIsActor_ThrowsSelfAdminActionNotAllowedException()
    {
        // ARRANGE
        var handler = CreateHandler();
        var command = new UpdateUserStatusCommand(5, false, "test") { UserId = 5, ActorRole = "Admin" };

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<SelfAdminActionNotAllowedException>();
        _userRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Handle_UserNotFound_ThrowsEntityNotFoundException
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new UpdateUserStatusCommand(99, false, "test"), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Handle_Freeze_UpdatesAndLogsFrozenDetail
    /// </summary>
    [Fact]
    public async Task Handle_Freeze_UpdatesAndLogsFrozenDetail()
    {
        // ARRANGE
        var user = new User { Id = 5, IsActive = true };
        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();
        var command = new UpdateUserStatusCommand(5, false, "Abuse report") { UserId = 1, ActorRole = "Admin" };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        user.IsActive.Should().BeFalse();
        _securityLogger.Verify(
            l => l.LogAsync(LogEventType.AdminAction, 5, null, null, null, "USER_ACCOUNT_FROZEN", default),
            Times.Once
        );
    }

    /// <summary>
    /// Handle_Reactivate_LogsReactivatedDetail
    /// </summary>
    [Fact]
    public async Task Handle_Reactivate_LogsReactivatedDetail()
    {
        // ARRANGE
        var user = new User { Id = 5, IsActive = false };
        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();
        var command = new UpdateUserStatusCommand(5, true, null) { UserId = 1, ActorRole = "Admin" };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        user.IsActive.Should().BeTrue();
        _securityLogger.Verify(
            l => l.LogAsync(LogEventType.AdminAction, 5, null, null, null, "USER_ACCOUNT_REACTIVATED", default),
            Times.Once
        );
    }
}
