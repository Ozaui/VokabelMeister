using Moq;
using FluentAssertions;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Enums.Logging;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Auth;

public class ConfirmAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private ConfirmAccountDeletionCommandHandler CreateHandler() =>
        new(_userRepository.Object, _refreshTokenRepository.Object, _otpService.Object, _securityLogger.Object);

    [Fact]
    public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(99, "123456", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsOtpExpiredExceptionAndLogsOtpFailed()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.Expired);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(1, "123456", "TestAgent/1.0", "9.9.9.9"), default);

        // ASSERT
        await act.Should().ThrowAsync<OtpExpiredException>();
        _securityLogger.Verify(s => s.LogAsync(LogEventType.OtpFailed, 1, "ada@test.de", "9.9.9.9", "TestAgent/1.0", "OTP_EXPIRED", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOtpExceptionAndLogsOtpFailed()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "000000", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.InvalidCode);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(1, "000000", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
        _securityLogger.Verify(s => s.LogAsync(LogEventType.OtpFailed, 1, "ada@test.de", null, null, "INVALID_OTP", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCode_StartsGracePeriodRevokesAllTokensAndLogsAccountDeletion()
    {
        // ARRANGE — kalıcı anonimleştirme DEĞİL, 30 gün grace period başlar (SECURITY.md §9)
        var user = new User { Id = 1, Email = "ada@test.de", IsActive = true };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.Success);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ConfirmAccountDeletionCommand(1, "123456", "TestAgent/1.0", "9.9.9.9"), default);

        // ASSERT
        user.IsDeleted.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        user.ScheduledDeletionAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
        _refreshTokenRepository.Verify(r => r.RevokeAllForUserAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // SecurityLog: A-04'te eklenen AccountDeletion başarı olayı
        _securityLogger.Verify(s => s.LogAsync(LogEventType.AccountDeletion, 1, "ada@test.de", "9.9.9.9", "TestAgent/1.0", "ACCOUNT_DELETED", It.IsAny<CancellationToken>()), Times.Once);
    }
}
