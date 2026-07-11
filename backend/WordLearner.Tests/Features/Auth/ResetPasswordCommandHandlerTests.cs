// ─────────────────────────────────────────────────────────────────────────────
// ResetPasswordCommandHandlerTests.cs
//
// AMAÇ: ResetPasswordCommandHandler'ın OTP + yeni şifre ile şifre güncelleme ve
//       tüm cihazlardan çıkış yapma davranışını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ResetPasswordCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _emailService.Object
        );

    /// <summary>
    /// ResetPasswordAsync_ValidOtp_UpdatesPasswordAndRevokesAllRefreshTokens
    ///
    /// AMAÇ: Doğru OTP ile şifrenin güncellendiğini ve kullanıcının TÜM refresh
    ///       token'larının iptal edildiğini (tüm cihazlardan çıkış) doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_ValidOtp_UpdatesPasswordAndRevokesAllRefreshTokens()
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

    /// <summary>
    /// ResetPasswordAsync_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP ile şifre sıfırlama denendiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_WrongOtp_ThrowsInvalidOtpException()
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

    /// <summary>
    /// ResetPasswordAsync_GermanLanguage_ReturnsGermanMessage
    ///
    /// AMAÇ: Command'a Language="de" verildiğinde MessageResponse.Message'ın Almanca
    ///       döndüğünü doğrulamak (A-03.2 — başarı mesajı lokalizasyonu).
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_GermanLanguage_ReturnsGermanMessage()
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
