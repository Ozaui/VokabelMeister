using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private LoginCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _emailService.Object,
            _securityLogger.Object
        );

    private static User CreateActiveUser(string email = "test@example.com", string? passwordHash = "hash") =>
        new()
        {
            Id = 1,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            IsEmailVerified = true,
        };

    [Fact]
    public async Task Login_ValidCredentials_SendsLoginOtp()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginCommand(user.Email, "Deneme123!@#"), default);

        // ASSERT
        _emailService.Verify(e => e.SendLoginOtpAsync(user.Email, "123456", default), Times.Once);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand(user.Email, "YanlisSifre"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Login_WrongPassword_LogsLoginFailedSecurityEvent()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand(user.Email, "YanlisSifre") { ClientIp = "1.2.3.4" }, default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _securityLogger.Verify(
            s => s.LogAsync(
                LogEventType.LoginFailed,
                user.Id,
                user.Email,
                "1.2.3.4",
                null,
                null,
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Login_UserNotFound_StillCallsVerifyWithFakeHashForTimingSafety()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand("yok@example.com", "HerhangiBirSifre123!"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _passwordService.Verify(p => p.Verify("HerhangiBirSifre123!", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Login_AccountNotActive_ThrowsAccountNotActiveException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.IsActive = false;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand(user.Email, "Deneme123!@#"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountNotActiveException>();
    }

    [Fact]
    public async Task Login_GermanLanguage_ReturnsGermanOtpSentMessage()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new LoginCommand(user.Email, "Deneme123!@#") { Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("OTP_SENT");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("OTP_SENT", "de"));
    }
}
