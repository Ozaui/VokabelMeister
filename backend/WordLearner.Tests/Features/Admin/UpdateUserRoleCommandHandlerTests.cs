// ─────────────────────────────────────────────────────────────────────────────
// UpdateUserRoleCommandHandlerTests.cs
//
// AMAÇ: UpdateUserRoleCommandHandler'ın rolü güncellediğini, hem IActivityLogger
//       (UPDATE_USER_ROLE) hem ISecurityLogger (AdminAction) çağırdığını
//       doğrulamak — CLAUDE.md "admin'e özel hassas işlem" kuralı.
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

public class UpdateUserRoleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private UpdateUserRoleCommandHandler CreateHandler() =>
        new(_userRepo.Object, _activityLogger.Object, _securityLogger.Object);

    /// <summary>
    /// Handle_TargetIsActor_ThrowsSelfAdminActionNotAllowedException
    ///
    /// AMAÇ: Bir admin kendi rolünü değiştiremez — kaza sonucu kendini User'a
    ///       düşürüp kilitlenmesin diye. Repository'e hiç ULAŞILMADIĞI da doğrulanır.
    /// </summary>
    [Fact]
    public async Task Handle_TargetIsActor_ThrowsSelfAdminActionNotAllowedException()
    {
        // ARRANGE
        var handler = CreateHandler();
        var command = new UpdateUserRoleCommand(5, "User") { UserId = 5, ActorRole = "Admin" };

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
        var act = () => handler.Handle(new UpdateUserRoleCommand(99, "Admin"), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Handle_ValidRole_UpdatesAndLogsBoth
    /// </summary>
    [Fact]
    public async Task Handle_ValidRole_UpdatesAndLogsBoth()
    {
        // ARRANGE
        var user = new User { Id = 5, Role = "User" };
        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();
        var command = new UpdateUserRoleCommand(5, "Admin") { UserId = 1, ActorRole = "Admin", IpAddress = "1.2.3.4" };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        user.Role.Should().Be("Admin");
        _userRepo.Verify(r => r.UpdateAsync(user, 1, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(
            l => l.LogAsync(1, "Admin", "UPDATE_USER_ROLE", "User", 5, It.IsAny<object>(), It.IsAny<object>(), "1.2.3.4", null, default),
            Times.Once
        );
        _securityLogger.Verify(
            l => l.LogAsync(LogEventType.AdminAction, 5, null, "1.2.3.4", null, "USER_ROLE_CHANGED", default),
            Times.Once
        );
    }
}
