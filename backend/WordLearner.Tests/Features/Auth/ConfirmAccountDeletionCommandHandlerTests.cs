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

public class ConfirmAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private ConfirmAccountDeletionCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _securityLogger.Object
        );

    private static User CreateActiveUser(string passwordHash = "hash") =>
        new()
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = passwordHash,
            IsActive = true,
        };

    [Fact]
    public async Task ConfirmAccountDeletion_ValidOtpAndPassword_SoftDeletesAndSchedulesAnonymization()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.AccountDeletion));
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ConfirmAccountDeletionCommand("123456", "Deneme123!@#") { UserId = user.Id }, default);

        // ASSERT
        user.IsDeleted.Should().BeTrue();
        user.ScheduledDeletionAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
        _refreshTokenRepo.Verify(r => r.RevokeAllForUserAsync(user.Id, default), Times.Once);
    }

    [Fact]
    public async Task ConfirmAccountDeletion_ValidOtpAndPassword_LogsAccountDeletionSecurityEvent()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.AccountDeletion));
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(
            new ConfirmAccountDeletionCommand("123456", "Deneme123!@#") { UserId = user.Id, ClientIp = "1.2.3.4" },
            default
        );

        // ASSERT
        _securityLogger.Verify(
            s => s.LogAsync(LogEventType.AccountDeletion, user.Id, null, "1.2.3.4", null, null, default),
            Times.Once
        );
    }

    [Fact]
    public async Task ConfirmAccountDeletion_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.AccountDeletion));
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(new ConfirmAccountDeletionCommand("123456", "YanlisSifre") { UserId = user.Id }, default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task ConfirmAccountDeletion_WrongPassword_LogsLoginFailedSecurityEvent()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.AccountDeletion));
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(
                new ConfirmAccountDeletionCommand("123456", "YanlisSifre")
                {
                    UserId = user.Id,
                    ClientIp = "1.2.3.4",
                },
                default
            );

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _securityLogger.Verify(
            s => s.LogAsync(
                LogEventType.LoginFailed,
                user.Id,
                null,
                "1.2.3.4",
                null,
                "ACCOUNT_DELETION_PASSWORD_MISMATCH",
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConfirmAccountDeletion_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.AccountDeletion)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(new ConfirmAccountDeletionCommand("999999", "Deneme123!@#") { UserId = user.Id }, default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task ConfirmAccountDeletion_WrongOtp_LogsOtpFailedSecurityEvent()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "999999", OtpPurpose.AccountDeletion)).Throws<InvalidOtpException>();
        var handler = CreateHandler();

        // ACT
        var act = () =>
            handler.Handle(
                new ConfirmAccountDeletionCommand("999999", "Deneme123!@#")
                {
                    UserId = user.Id,
                    ClientIp = "1.2.3.4",
                },
                default
            );

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
        _securityLogger.Verify(
            s => s.LogAsync(
                LogEventType.OtpFailed,
                user.Id,
                null,
                "1.2.3.4",
                null,
                "AccountDeletion",
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConfirmAccountDeletion_GermanLanguage_ReturnsGermanMessage()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Validate(user, "123456", OtpPurpose.AccountDeletion));
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new ConfirmAccountDeletionCommand("123456", "Deneme123!@#") { UserId = user.Id, Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("ACCOUNT_DELETION_CONFIRMED");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("ACCOUNT_DELETION_CONFIRMED", "de"));
    }
}
