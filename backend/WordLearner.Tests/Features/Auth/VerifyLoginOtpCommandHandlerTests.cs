using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Auth;

public class VerifyLoginOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private VerifyLoginOtpCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _loginCompletionService.Object, _securityLogger.Object);

    [Fact]
    public async Task VerifyLoginOtp_ValidOtp_DelegatesToLoginCompletionService()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", CurrentLevel = "A1" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.LoginOtp));
        var beklenenYanit = new AuthTokenResponse(
            "access-token",
            "refresh-token",
            900,
            new AuthUserDto(user.Id, user.CurrentLevel, user.ThemePreference),
            false
        );
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(user, "1.2.3.4", null, default))
            .ReturnsAsync(beklenenYanit);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new VerifyLoginOtpCommand(user.Email, "123456") { ClientIp = "1.2.3.4" },
            default
        );

        // ASSERT
        sonuc.Should().Be(beklenenYanit);
        _loginCompletionService.Verify(l => l.CompleteLoginAsync(user, "1.2.3.4", null, default), Times.Once);
    }

    [Fact]
    public async Task VerifyLoginOtp_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.LoginOtp)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyLoginOtpCommand(user.Email, "999999"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task VerifyLoginOtp_WrongOtp_LogsOtpFailedSecurityEvent()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.LoginOtp)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(
                new VerifyLoginOtpCommand(user.Email, "999999") { ClientIp = "1.2.3.4" },
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
                "LoginOtp",
                default
            ),
            Times.Once
        );
    }
}
