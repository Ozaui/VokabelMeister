// ─────────────────────────────────────────────────────────────────────────────
// VerifyEmailCommandHandlerTests.cs
//
// AMAÇ: VerifyEmailCommandHandler'ın OTP doğrulama + e-posta aktive etme akışını
//       doğrulamak.
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

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();

    private VerifyEmailCommandHandler CreateHandler() => new(_userRepo.Object, _otpService.Object);

    /// <summary>
    /// VerifyEmailAsync_ValidOtp_MarksEmailVerifiedAndClearsOtp
    ///
    /// AMAÇ: Doğru OTP kodu girildiğinde IsEmailVerified'ın true'ya çekildiğini ve
    ///       bekleyen OTP alanlarının temizlendiğini doğrulamak.
    /// NEDEN: Bu adım tamamlanmadan hesap login akışında (IsActive kontrolü ayrı, ama
    ///        onboarding vb. akışlarda) doğrulanmamış olarak kalır.
    /// </summary>
    [Fact]
    public async Task VerifyEmailAsync_ValidOtp_MarksEmailVerifiedAndClearsOtp()
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
        _userRepo.Verify(r => r.UpdateAsync(user, null, default), Times.Once);
    }

    /// <summary>
    /// VerifyEmailAsync_WrongOtpCode_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP kodu girildiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// NEDEN: IOtpService.Validate'in hash karşılaştırması EmailVerification akışında da geçerli olmalı.
    /// </summary>
    [Fact]
    public async Task VerifyEmailAsync_WrongOtpCode_ThrowsInvalidOtpException()
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
}
