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

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private VerifyEmailCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _securityLogger.Object);

    [Fact]
    public async Task VerifyEmail_ValidOtp_MarksEmailVerifiedAndClearsOtp()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.EmailVerification));
        _otpService
            .Setup(o => o.Clear(user))
            .Callback<User>(u =>
            {
                u.PendingOtpCodeHash = null;
                u.PendingOtpCodeExpiresAt = null;
                u.PendingOtpCodePurpose = null;
            });
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new VerifyEmailCommand("test@example.com", "123456"), default);

        // ASSERT
        user.IsEmailVerified.Should().BeTrue();
        user.PendingOtpCodeHash.Should().BeNull();
        _userRepo.Verify(r => r.UpdateAsync(user, user.Id, default), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_WrongOtpCode_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _otpService
            .Setup(o => o.Validate(user, "999999", OtpPurpose.EmailVerification))
            .Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyEmailCommand("test@example.com", "999999"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task VerifyEmail_WrongOtpCode_LogsOtpFailedSecurityEvent()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _otpService
            .Setup(o => o.Validate(user, "999999", OtpPurpose.EmailVerification))
            .Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(
                new VerifyEmailCommand("test@example.com", "999999") { ClientIp = "1.2.3.4" },
                default
            );

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
        _securityLogger.Verify(
            s => s.LogAsync(
                LogEventType.OtpFailed,
                user.Id,
                "test@example.com",
                "1.2.3.4",
                null,
                "EmailVerification",
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VerifyEmail_GermanLanguage_ReturnsGermanMessage()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.EmailVerification));
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new VerifyEmailCommand(user.Email, "123456") { Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("EMAIL_VERIFIED");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("EMAIL_VERIFIED", "de"));
    }
}
