using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Exceptions;

namespace WordLearner.Tests.Features.Auth;

public class ConfirmAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IOtpService> _otpService = new();

    private ConfirmAccountDeletionCommandHandler CreateHandler() =>
        new(_userRepository.Object, _refreshTokenRepository.Object, _otpService.Object);

    [Fact]
    public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(99, "123456"), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsOtpExpiredException()
    {
        // ARRANGE
        var user = new User { Id = 1 };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.Expired);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(1, "123456"), default);

        // ASSERT
        await act.Should().ThrowAsync<OtpExpiredException>();
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Id = 1 };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "000000", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.InvalidCode);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmAccountDeletionCommand(1, "000000"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_ValidCode_StartsGracePeriodAndRevokesAllTokens()
    {
        // ARRANGE — kalıcı anonimleştirme DEĞİL, 30 gün grace period başlar (SECURITY.md §9)
        var user = new User { Id = 1, IsActive = true };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.AccountDeletion)).Returns(OtpVerificationResult.Success);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ConfirmAccountDeletionCommand(1, "123456"), default);

        // ASSERT
        user.IsDeleted.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        user.ScheduledDeletionAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
        _refreshTokenRepository.Verify(r => r.RevokeAllForUserAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
