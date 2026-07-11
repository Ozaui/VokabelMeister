// ─────────────────────────────────────────────────────────────────────────────
// VerifyLoginOtpCommandHandlerTests.cs
//
// AMAÇ: VerifyLoginOtpCommandHandler'ın OTP doğrulaması yapıp ILoginCompletionService'e
//       doğru şekilde delege ettiğini doğrulamak.
// NEDEN: Grace period kurtarma/giriş istatistikleri gibi CompleteLoginAsync'in
//        kendi iç davranışı artık LoginCompletionServiceTests'te kapsanıyor —
//        burada yalnızca handler'ın doğru kullanıcıyla/ip'yle delege ettiği
//        ve OTP hatasını doğru fırlattığı test edilir (kod tekrarını önler).
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class VerifyLoginOtpCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();

    private VerifyLoginOtpCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _loginCompletionService.Object);

    /// <summary>
    /// VerifyLoginOtp_ValidOtp_DelegatesToLoginCompletionService
    ///
    /// AMAÇ: Doğru OTP kodu girildiğinde ILoginCompletionService.CompleteLoginAsync'in
    ///       doğru kullanıcı/ip ile çağrıldığını ve sonucunun aynen döndüğünü doğrulamak.
    /// </summary>
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
            new AuthUserDto(user.Id, user.CurrentLevel),
            false
        );
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(user, "1.2.3.4", default))
            .ReturnsAsync(beklenenYanit);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new VerifyLoginOtpCommand(user.Email, "123456") { ClientIp = "1.2.3.4" },
            default
        );

        // ASSERT
        sonuc.Should().Be(beklenenYanit);
        _loginCompletionService.Verify(l => l.CompleteLoginAsync(user, "1.2.3.4", default), Times.Once);
    }

    /// <summary>
    /// VerifyLoginOtp_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP kodu girildiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// </summary>
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
}
