using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ResetPasswordCommandHandler CreateHandler() => new(
        _userRepository.Object, _refreshTokenRepository.Object, _otpService.Object, _passwordService.Object, _emailService.Object);

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOtpException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ResetPasswordCommand("yok@test.de", "123456", "YeniSifre1!", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsOtpExpiredException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.PasswordReset)).Returns(OtpVerificationResult.Expired);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ResetPasswordCommand("ada@test.de", "123456", "YeniSifre1!", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<OtpExpiredException>();
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "000000", OtpPurpose.PasswordReset)).Returns(OtpVerificationResult.InvalidCode);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ResetPasswordCommand("ada@test.de", "000000", "YeniSifre1!", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_ValidCode_UpdatesPasswordRevokesTokensAndNotifies()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de", FirstName = "Ada" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.PasswordReset)).Returns(OtpVerificationResult.Success);
        _passwordService.Setup(p => p.Hash("YeniSifre1!")).Returns("yeni-hash");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResetPasswordCommand("ada@test.de", "123456", "YeniSifre1!", "tr"), default);

        // ASSERT — SECURITY.md §7: şifre sıfırlanınca tüm cihazlardan çıkış
        user.PasswordHash.Should().Be("yeni-hash");
        _refreshTokenRepository.Verify(r => r.RevokeAllForUserAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendPasswordChangedNotificationAsync("ada@test.de", "Ada", "tr", It.IsAny<CancellationToken>()), Times.Once);
    }
}
