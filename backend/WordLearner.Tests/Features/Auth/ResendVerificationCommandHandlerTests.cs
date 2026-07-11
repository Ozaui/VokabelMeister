// ─────────────────────────────────────────────────────────────────────────────
// ResendVerificationCommandHandlerTests.cs
//
// AMAÇ: ResendVerificationCommandHandler'ın var olan/olmayan kullanıcı
//       senaryolarını (e-posta numaralandırma önlemi dahil) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class ResendVerificationCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ResendVerificationCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _emailService.Object);

    /// <summary>
    /// ResendVerificationAsync_UnverifiedUserExists_SendsNewOtp
    ///
    /// AMAÇ: Doğrulanmamış bir kullanıcı için yeni OTP üretilip e-posta gönderildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResendVerificationAsync_UnverifiedUserExists_SendsNewOtp()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com", IsEmailVerified = false };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResendVerificationCommand("test@example.com"), default);

        // ASSERT
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync("test@example.com", "123456", default), Times.Once);
    }

    /// <summary>
    /// ResendVerificationAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage
    ///
    /// AMAÇ: Kayıtlı olmayan bir e-posta için de aynı mesajın döndüğünü ama e-posta
    ///       GÖNDERİLMEDİĞİNİ doğrulamak.
    /// NEDEN: E-posta numaralandırma (enumeration) saldırısını önlemek için — yanıt
    ///        farklı olsaydı bir saldırgan hangi e-postaların kayıtlı olduğunu anlayabilirdi.
    /// </summary>
    [Fact]
    public async Task ResendVerificationAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new ResendVerificationCommand("yok@example.com"), default);

        // ASSERT
        sonuc.Message.Should().NotBeNullOrEmpty();
        _emailService.Verify(
            e => e.SendEmailVerificationOtpAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never
        );
    }

    /// <summary>
    /// ResendVerificationAsync_GermanLanguage_ReturnsGermanMessage
    ///
    /// AMAÇ: Command'a Language="de" verildiğinde MessageResponse.Message'ın Almanca
    ///       döndüğünü doğrulamak (A-03.2 — başarı mesajı lokalizasyonu).
    /// </summary>
    [Fact]
    public async Task ResendVerificationAsync_GermanLanguage_ReturnsGermanMessage()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com", IsEmailVerified = false };
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new ResendVerificationCommand(user.Email) { Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("VERIFICATION_CODE_SENT");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("VERIFICATION_CODE_SENT", "de"));
    }
}
