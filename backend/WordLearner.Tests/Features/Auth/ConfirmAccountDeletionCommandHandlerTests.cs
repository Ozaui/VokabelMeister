// ─────────────────────────────────────────────────────────────────────────────
// ConfirmAccountDeletionCommandHandlerTests.cs
//
// AMAÇ: ConfirmAccountDeletionCommandHandler'ın çift onay (OTP + şifre) ile soft
//       delete + 30 gün grace period zamanlama davranışını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class ConfirmAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();

    private ConfirmAccountDeletionCommandHandler CreateHandler() =>
        new(_userRepo.Object, _refreshTokenRepo.Object, _passwordService.Object, _otpService.Object);

    private static User CreateActiveUser(string passwordHash = "hash") =>
        new()
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = passwordHash,
            IsActive = true,
        };

    /// <summary>
    /// ConfirmAccountDeletionAsync_ValidOtpAndPassword_SoftDeletesAndSchedulesAnonymization
    ///
    /// AMAÇ: Doğru OTP + doğru şifre ile hesabın soft-delete edildiğini ve 30 gün
    ///       sonrasına anonimleştirme zamanlandığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_ValidOtpAndPassword_SoftDeletesAndSchedulesAnonymization()
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

    /// <summary>
    /// ConfirmAccountDeletionAsync_WrongPassword_ThrowsInvalidCredentialsException
    ///
    /// AMAÇ: OTP doğru olsa bile yanlış şifre ile hesap silmenin onaylanamadığını doğrulamak.
    /// NEDEN: Geri alınamaz bir işlem olduğu için çift onay (OTP + şifre) zorunlu.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_WrongPassword_ThrowsInvalidCredentialsException()
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

    /// <summary>
    /// ConfirmAccountDeletionAsync_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP ile hesap silme onayının reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_WrongOtp_ThrowsInvalidOtpException()
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
}
