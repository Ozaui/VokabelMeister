using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepository.Object, _passwordService.Object, _otpService.Object, _emailService.Object, _securityLogger.Object);

    [Fact]
    public async Task Handle_UserNotFound_StillVerifiesDummyHashAndThrowsInvalidCredentialsException()
    {
        // ARRANGE — SECURITY.md §1: kayıtsız e-postada da bcrypt karşılaştırması sabit süre için ÇALIŞTIRILIR
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.Verify("Sifre123!", It.IsAny<string>())).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand("yok@test.de", "Sifre123!", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _passwordService.Verify(p => p.Verify("Sifre123!", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentialsExceptionAndLogsLoginFailed()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", PasswordHash = "gercek-hash" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("YanlisSifre", "gercek-hash")).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand("ada@test.de", "YanlisSifre", "TestAgent/1.0", "9.9.9.9", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        // SecurityLog: yanlış şifre A-04'te eklenen LoginFailed olayını, ham e-posta ve IP/UserAgent ile tetiklemeli
        _securityLogger.Verify(s => s.LogAsync(LogEventType.LoginFailed, null, "ada@test.de", "9.9.9.9", "TestAgent/1.0", "INVALID_CREDENTIALS", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SocialOnlyAccount_ThrowsInvalidCredentialsException()
    {
        // ARRANGE — PasswordHash null: sosyal-yalnızca hesap, şifreyle giriş yapılamaz
        var user = new User { Email = "ada@test.de", PasswordHash = null };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand("ada@test.de", "Sifre123!", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_InactiveAccount_ThrowsAccountInactiveException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", PasswordHash = "gercek-hash", IsActive = false };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Sifre123!", "gercek-hash")).Returns(true);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginCommand("ada@test.de", "Sifre123!", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountInactiveException>();
    }

    [Fact]
    public async Task Handle_ValidCredentials_GeneratesOtpAndSendsLoginEmail()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", FirstName = "Ada", PasswordHash = "gercek-hash", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Sifre123!", "gercek-hash")).Returns(true);
        _otpService.Setup(o => o.Generate(user, OtpPurpose.LoginOtp)).Returns("111222");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginCommand("ada@test.de", "Sifre123!", null, null, "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendLoginOtpAsync("ada@test.de", "Ada", "111222", "tr", It.IsAny<CancellationToken>()), Times.Once);
    }
}
