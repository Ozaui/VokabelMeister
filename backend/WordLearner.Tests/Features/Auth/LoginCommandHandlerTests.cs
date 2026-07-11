// ─────────────────────────────────────────────────────────────────────────────
// LoginCommandHandlerTests.cs
//
// AMAÇ: LoginCommandHandler'ın (2 adımlı OTP login'in 1. adımı) şifre doğrulama,
//       timing-attack önlemi ve hesap durumu kontrollerini doğrulamak.
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

namespace WordLearner.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepo.Object, _passwordService.Object, _otpService.Object, _emailService.Object);

    private static User CreateActiveUser(string email = "test@example.com", string? passwordHash = "hash") =>
        new()
        {
            Id = 1,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            IsEmailVerified = true,
        };

    /// <summary>
    /// LoginAsync_ValidCredentials_SendsLoginOtp
    ///
    /// AMAÇ: Doğru şifre ile login adım 1'in OTP gönderdiğini (token DÖNMEDİĞİNİ) doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_SendsLoginOtp()
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

    /// <summary>
    /// LoginAsync_WrongPassword_ThrowsInvalidCredentialsException
    ///
    /// AMAÇ: Yanlış şifre girildiğinde InvalidCredentialsException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
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

    /// <summary>
    /// LoginAsync_UserNotFound_StillCallsVerifyWithFakeHashForTimingSafety
    ///
    /// AMAÇ: Kullanıcı bulunamadığında bile PasswordService.Verify'ın (FakePasswordHashForTiming
    ///       ile) ÇAĞRILDIĞINI ve InvalidCredentialsException fırlatıldığını doğrulamak.
    /// NEDEN: Timing attack önlemi — kullanıcı var/yok fark etmeksizin aynı süre harcanmalı;
    ///        Verify çağrılmazsa "kullanıcı yok" yanıtı ölçülebilir şekilde daha hızlı döner,
    ///        bu da bir saldırganın e-posta numaralandırması yapmasına izin verir.
    /// </summary>
    [Fact]
    public async Task LoginAsync_UserNotFound_StillCallsVerifyWithFakeHashForTimingSafety()
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

    /// <summary>
    /// LoginAsync_AccountNotActive_ThrowsAccountNotActiveException
    ///
    /// AMAÇ: Şifre doğru olsa bile dondurulmuş (IsActive=false) bir hesapla login
    ///       denendiğinde AccountNotActiveException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_AccountNotActive_ThrowsAccountNotActiveException()
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

    /// <summary>
    /// LoginAsync_GermanLanguage_ReturnsGermanOtpSentMessage
    ///
    /// AMAÇ: Command'a Language="de" verildiğinde MessageResponse.Message'ın Almanca
    ///       döndüğünü doğrulamak (A-03.2 — başarı mesajı lokalizasyonu).
    /// </summary>
    [Fact]
    public async Task LoginAsync_GermanLanguage_ReturnsGermanOtpSentMessage()
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
