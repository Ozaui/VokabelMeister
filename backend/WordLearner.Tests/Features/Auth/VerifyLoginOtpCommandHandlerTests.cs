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

public class VerifyLoginOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private VerifyLoginOtpCommandHandler CreateHandler() => new(
        _userRepository.Object, _refreshTokenRepository.Object, _otpService.Object,
        _loginCompletionService.Object, _emailService.Object, _securityLogger.Object);

    private static LoginCompletionResult CreateCompletion(bool recovered = false) =>
        new("access-token", "refresh-token", new RefreshToken { UserId = 1 }, recovered);

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOtpException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyLoginOtpCommand("yok@test.de", "123456", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_InactiveAccount_ThrowsAccountInactiveException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", IsActive = false };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyLoginOtpCommand("ada@test.de", "123456", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountInactiveException>();
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsOtpExpiredExceptionAndLogsOtpFailed()
    {
        // ARRANGE
        var user = new User { Id = 7, Email = "ada@test.de", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.LoginOtp)).Returns(OtpVerificationResult.Expired);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyLoginOtpCommand("ada@test.de", "123456", "TestAgent/1.0", "9.9.9.9", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<OtpExpiredException>();
        // SecurityLog: dört OTP akışının paylaştığı OtpFailed olayı, süresi dolmuş kod için OTP_EXPIRED Code'uyla
        _securityLogger.Verify(s => s.LogAsync(LogEventType.OtpFailed, 7, "ada@test.de", "9.9.9.9", "TestAgent/1.0", "OTP_EXPIRED", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOtpExceptionAndLogsOtpFailed()
    {
        // ARRANGE
        var user = new User { Id = 7, Email = "ada@test.de", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "000000", OtpPurpose.LoginOtp)).Returns(OtpVerificationResult.InvalidCode);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyLoginOtpCommand("ada@test.de", "000000", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
        _securityLogger.Verify(s => s.LogAsync(LogEventType.OtpFailed, 7, "ada@test.de", null, null, "INVALID_OTP", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCode_ReturnsLoginResponseAndPersistsRefreshToken()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de", IsActive = true, CurrentLevel = "A1", ThemePreference = "System", LanguagePreference = "tr" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.LoginOtp)).Returns(OtpVerificationResult.Success);
        var completion = CreateCompletion();
        _loginCompletionService.Setup(l => l.Complete(user, "Chrome", "1.2.3.4")).Returns(completion);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new VerifyLoginOtpCommand("ada@test.de", "123456", "Chrome", "1.2.3.4", "tr"), default);

        // ASSERT
        _refreshTokenRepository.Verify(r => r.AddAsync(completion.RefreshTokenEntity, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.AccessToken.Should().Be("access-token");
        result.User.Id.Should().Be(1);
        _emailService.Verify(e => e.SendAccountRecoveredNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccountWasRecovered_SendsAccountRecoveredNotification()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de", FirstName = "Ada", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.LoginOtp)).Returns(OtpVerificationResult.Success);
        _loginCompletionService.Setup(l => l.Complete(user, null, null)).Returns(CreateCompletion(recovered: true));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new VerifyLoginOtpCommand("ada@test.de", "123456", null, null, "de"), default);

        // ASSERT
        _emailService.Verify(e => e.SendAccountRecoveredNotificationAsync("ada@test.de", "Ada", "de", It.IsAny<CancellationToken>()), Times.Once);
    }
}
