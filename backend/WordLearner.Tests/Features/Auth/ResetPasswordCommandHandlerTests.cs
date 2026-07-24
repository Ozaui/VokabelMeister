using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private ResetPasswordCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _emailService.Object,
            _securityLogger.Object
        );

    [Fact]
    public async Task ResetPassword_ValidOtp_UpdatesPasswordAndRevokesAllRefreshTokens()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.PasswordReset));
        _passwordService.Setup(p => p.Hash("YeniSifre123!@#")).Returns("yeni-hash");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResetPasswordCommand(user.Email, "123456", "YeniSifre123!@#"), default);

        // ASSERT
        user.PasswordHash.Should().Be("yeni-hash");
        _refreshTokenRepo.Verify(r => r.RevokeAllForUserAsync(user.Id, default), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ValidOtp_LogsPasswordResetSecurityEvent()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.PasswordReset));
        _passwordService.Setup(p => p.Hash("YeniSifre123!@#")).Returns("yeni-hash");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(
            new ResetPasswordCommand(user.Email, "123456", "YeniSifre123!@#") { ClientIp = "1.2.3.4" },
            default
        );

        // ASSERT
        _securityLogger.Verify(
            s => s.LogAsync(LogEventType.PasswordReset, user.Id, null, "1.2.3.4", null, null, default),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.PasswordReset)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ResetPasswordCommand(user.Email, "999999", "YeniSifre123!@#"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task ResetPassword_WrongOtp_LogsOtpFailedSecurityEvent()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.PasswordReset)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(
                new ResetPasswordCommand(user.Email, "999999", "YeniSifre123!@#") { ClientIp = "1.2.3.4" },
                default
            );

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
        _securityLogger.Verify(
            s => s.LogAsync(
                LogEventType.OtpFailed,
                user.Id,
                user.Email,
                "1.2.3.4",
                null,
                "PasswordReset",
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_GermanLanguage_ReturnsGermanMessage()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.PasswordReset));
        _passwordService.Setup(p => p.Hash("YeniSifre123!@#")).Returns("yeni-hash");
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new ResetPasswordCommand(user.Email, "123456", "YeniSifre123!@#") { Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("PASSWORD_UPDATED");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("PASSWORD_UPDATED", "de"));
    }
}
