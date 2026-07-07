// ─────────────────────────────────────────────────────────────────────────────
// ForgotPasswordCommandHandlerTests.cs
//
// AMAÇ: ForgotPasswordCommandHandler'ın var olan/olmayan kullanıcı senaryolarını
//       (e-posta numaralandırma önlemi dahil) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _emailService.Object);

    /// <summary>
    /// ForgotPasswordAsync_ExistingUser_SendsResetOtp
    ///
    /// AMAÇ: Kayıtlı bir kullanıcı için şifre sıfırlama OTP'sinin gönderildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_SendsResetOtp()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ForgotPasswordCommand(user.Email), default);

        // ASSERT
        _emailService.Verify(e => e.SendPasswordResetOtpAsync(user.Email, "123456", default), Times.Once);
    }

    /// <summary>
    /// ForgotPasswordAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage
    ///
    /// AMAÇ: Kayıtlı olmayan bir e-posta için de e-posta GÖNDERİLMEDİĞİNİ ama aynı
    ///       mesajın döndüğünü doğrulamak (e-posta numaralandırma önlemi).
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new ForgotPasswordCommand("yok@example.com"), default);

        // ASSERT
        sonuc.Message.Should().NotBeNullOrEmpty();
        _emailService.Verify(
            e => e.SendPasswordResetOtpAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never
        );
    }
}
